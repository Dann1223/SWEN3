using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PaperlessRESTAPI.Controllers;
using PaperlessRESTAPI.Data.Entities;
using PaperlessRESTAPI.Data.Mapping;
using PaperlessRESTAPI.Data.Repositories;
using PaperlessRESTAPI.Models.DTOs;
using System.Linq.Expressions;

namespace PaperlessRESTAPI.Tests.Controllers;

/// <summary>
/// Unit tests for TagsController
/// </summary>
public class TagsControllerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<Tag>> _mockTagRepository;
    private readonly Mock<ILogger<TagsController>> _mockLogger;
    private readonly IMapper _mapper;
    private readonly TagsController _controller;

    public TagsControllerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockTagRepository = new Mock<IRepository<Tag>>();
        _mockLogger = new Mock<ILogger<TagsController>>();

        // Setup AutoMapper
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
        _mapper = configuration.CreateMapper();

        // Setup mock unit of work
        _mockUnitOfWork.Setup(u => u.Tags).Returns(_mockTagRepository.Object);

        _controller = new TagsController(_mockUnitOfWork.Object, _mapper, _mockLogger.Object);
    }

    [Fact]
    public async Task GetTags_ShouldReturnAllTags()
    {
        // Arrange
        var tags = new List<Tag>
        {
            CreateTestTag(1, "Important", "#dc3545"),
            CreateTestTag(2, "Archive", "#6c757d")
        };

        _mockTagRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(tags);

        // Act
        var result = await _controller.GetTags();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedTags = okResult!.Value as IEnumerable<TagDto>;
        returnedTags.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTag_WithValidId_ShouldReturnTag()
    {
        // Arrange
        var tagId = 1;
        var tag = CreateTestTag(tagId, "Important", "#dc3545");

        _mockTagRepository.Setup(r => r.GetByIdAsync(tagId))
            .ReturnsAsync(tag);

        // Act
        var result = await _controller.GetTag(tagId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedTag = okResult!.Value as TagDto;
        returnedTag!.Id.Should().Be(tagId);
        returnedTag.Name.Should().Be("Important");
    }

    [Fact]
    public async Task GetTag_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var tagId = 999;
        _mockTagRepository.Setup(r => r.GetByIdAsync(tagId))
            .ReturnsAsync((Tag?)null);

        // Act
        var result = await _controller.GetTag(tagId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateTag_WithValidDto_ShouldReturnCreatedResult()
    {
        // Arrange
        var tagDto = new TagDto
        {
            Name = "Test Tag",
            Description = "Test Description",
            Color = "#ff0000"
        };

        var tag = new Tag { Id = 1, Name = "Test Tag", Description = "Test Description", Color = "#ff0000", CreatedDate = DateTime.UtcNow };
        var createdTagDto = new TagDto { Id = 1, Name = "Test Tag", Description = "Test Description", Color = "#ff0000", CreatedDate = DateTime.UtcNow };

        _mockUnitOfWork.Setup(u => u.Tags.FirstOrDefaultAsync(It.IsAny<Expression<Func<Tag, bool>>>()))
                  .ReturnsAsync((Tag?)null);
        _mockUnitOfWork.Setup(u => u.Tags.AddAsync(It.IsAny<Tag>())).ReturnsAsync(tag);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.CreateTag(tagDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedTag = createdResult.Value.Should().BeOfType<TagDto>().Subject;
        returnedTag.Name.Should().Be("Test Tag");
        returnedTag.Description.Should().Be("Test Description");
        returnedTag.Color.Should().Be("#ff0000");
    }

    [Fact]
    public async Task UpdateTag_WithValidData_ShouldReturnUpdatedTag()
    {
        // Arrange
        var tagId = 1;
        var existingTag = CreateTestTag(tagId, "Old Name", "#007bff");
        var updateDto = new TagDto
        {
            Id = tagId,
            Name = "Updated Tag",
            Description = "Updated description",
            Color = "#28a745"
        };

        _mockTagRepository.Setup(r => r.GetByIdAsync(tagId))
            .ReturnsAsync(existingTag);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _controller.UpdateTag(tagId, updateDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedTag = okResult!.Value as TagDto;
        returnedTag!.Name.Should().Be("Updated Tag");

        _mockTagRepository.Verify(r => r.Update(It.IsAny<Tag>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateTag_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var tagId = 999;
        var updateDto = new TagDto { Id = tagId, Name = "New Name" };

        _mockTagRepository.Setup(r => r.GetByIdAsync(tagId))
            .ReturnsAsync((Tag?)null);

        // Act
        var result = await _controller.UpdateTag(tagId, updateDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteTag_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var tagId = 1;
        var tag = CreateTestTag(tagId, "Test Tag", "#007bff");

        _mockTagRepository.Setup(r => r.GetByIdAsync(tagId))
            .ReturnsAsync(tag);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _controller.DeleteTag(tagId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockTagRepository.Verify(r => r.Delete(It.IsAny<Tag>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteTag_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var tagId = 999;
        _mockTagRepository.Setup(r => r.GetByIdAsync(tagId))
            .ReturnsAsync((Tag?)null);

        // Act
        var result = await _controller.DeleteTag(tagId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteTag_WhenSaveFails_ShouldStillReturnNoContent()
    {
        // Arrange
        var tagId = 1;
        var tag = CreateTestTag(tagId, "Test Tag", "#007bff");

        _mockTagRepository.Setup(r => r.GetByIdAsync(tagId))
            .ReturnsAsync(tag);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(0); // Simulate save failure

        // Act
        var result = await _controller.DeleteTag(tagId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void TagsController_ShouldHaveCorrectAttributes()
    {
        // Arrange
        var controllerType = typeof(TagsController);

        // Assert
        var apiControllerAttribute = controllerType.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.ApiControllerAttribute), false);
        var routeAttribute = controllerType.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.RouteAttribute), false);

        apiControllerAttribute.Should().HaveCount(1);
        routeAttribute.Should().HaveCount(1);
    }

    [Fact]
    public void TagsController_ShouldInheritFromControllerBase()
    {
        // Arrange & Assert
        typeof(TagsController).Should().BeDerivedFrom<ControllerBase>();
    }

    private static Tag CreateTestTag(int id, string name, string color)
    {
        return new Tag
        {
            Id = id,
            Name = name,
            Color = color,
            Description = $"Description for {name}",
            CreatedDate = DateTime.UtcNow
        };
    }
}
