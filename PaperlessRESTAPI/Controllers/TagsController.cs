using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PaperlessRESTAPI.Data.Entities;
using PaperlessRESTAPI.Data.Repositories;
using PaperlessRESTAPI.Models.DTOs;

namespace PaperlessRESTAPI.Controllers;

/// <summary>
/// Tags API Controller for managing document tags
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<TagsController> _logger;

    public TagsController(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<TagsController> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all tags
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetTags()
    {
        try
        {
            _logger.LogInformation("Retrieving all tags");
            
            var tags = await _unitOfWork.Tags.GetAllAsync();
            var tagDtos = _mapper.Map<IEnumerable<TagDto>>(tags);
            
            return Ok(tagDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tags");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get tag by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving tag with ID: {TagId}", id);
            
            var tag = await _unitOfWork.Tags.GetByIdAsync(id);
            
            if (tag == null)
            {
                return NotFound($"Tag with ID {id} not found");
            }

            var tagDto = _mapper.Map<TagDto>(tag);
            return Ok(tagDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tag with ID: {TagId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new tag
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag([FromBody] TagDto tagDto)
    {
        try
        {
            _logger.LogInformation("Creating new tag: {TagName}", tagDto.Name);

            // Check if tag name already exists
            var existingTag = await _unitOfWork.Tags.FirstOrDefaultAsync(t => t.Name == tagDto.Name);
            if (existingTag != null)
            {
                return BadRequest($"Tag with name '{tagDto.Name}' already exists");
            }

            var tag = _mapper.Map<Tag>(tagDto);
            tag.CreatedDate = DateTime.UtcNow;

            await _unitOfWork.Tags.AddAsync(tag);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Tag created successfully with ID: {TagId}", tag.Id);

            var createdTagDto = _mapper.Map<TagDto>(tag);
            return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, createdTagDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tag: {TagName}", tagDto.Name);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing tag
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TagDto>> UpdateTag(int id, [FromBody] TagDto tagDto)
    {
        try
        {
            _logger.LogInformation("Updating tag with ID: {TagId}", id);

            var existingTag = await _unitOfWork.Tags.GetByIdAsync(id);
            
            if (existingTag == null)
            {
                return NotFound($"Tag with ID {id} not found");
            }

            // Check if new name conflicts with existing tag (excluding current tag)
            if (existingTag.Name != tagDto.Name)
            {
                var conflictingTag = await _unitOfWork.Tags.FirstOrDefaultAsync(t => t.Name == tagDto.Name);
                if (conflictingTag != null)
                {
                    return BadRequest($"Tag with name '{tagDto.Name}' already exists");
                }
            }

            // Update properties
            existingTag.Name = tagDto.Name;
            existingTag.Description = tagDto.Description;
            existingTag.Color = tagDto.Color;

            _unitOfWork.Tags.Update(existingTag);
            await _unitOfWork.SaveChangesAsync();

            var updatedTagDto = _mapper.Map<TagDto>(existingTag);
            return Ok(updatedTagDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tag with ID: {TagId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a tag
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(int id)
    {
        try
        {
            _logger.LogInformation("Deleting tag with ID: {TagId}", id);

            var tag = await _unitOfWork.Tags.GetByIdAsync(id);
            
            if (tag == null)
            {
                return NotFound($"Tag with ID {id} not found");
            }

            _unitOfWork.Tags.Delete(tag);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Tag deleted successfully with ID: {TagId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tag with ID: {TagId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
