using Microsoft.AspNetCore.Mvc;
using PaperlessRESTAPI.Models.DTOs.Comments;
using PaperlessRESTAPI.Services.Interfaces;

namespace PaperlessRESTAPI.Controllers;

/// <summary>
/// Controller for document comment operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(ICommentService commentService, ILogger<CommentsController> logger)
    {
        _commentService = commentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all comments for a specific document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <param name="includeReplies">Whether to include nested replies</param>
    /// <returns>List of comments</returns>
    [HttpGet("document/{documentId:int}")]
    public async Task<ActionResult<List<CommentDto>>> GetDocumentComments(
        int documentId, 
        [FromQuery] bool includeReplies = true)
    {
        try
        {
            var comments = await _commentService.GetDocumentCommentsAsync(documentId, includeReplies);
            return Ok(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments for document {DocumentId}", documentId);
            return StatusCode(500, "An error occurred while retrieving comments");
        }
    }

    /// <summary>
    /// Get a specific comment by ID
    /// </summary>
    /// <param name="commentId">Comment ID</param>
    /// <param name="includeReplies">Whether to include nested replies</param>
    /// <returns>Comment details</returns>
    [HttpGet("{commentId:int}")]
    public async Task<ActionResult<CommentDto>> GetComment(
        int commentId, 
        [FromQuery] bool includeReplies = true)
    {
        try
        {
            var comment = await _commentService.GetCommentAsync(commentId, includeReplies);
            
            if (comment == null)
            {
                return NotFound($"Comment with ID {commentId} not found");
            }

            return Ok(comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comment {CommentId}", commentId);
            return StatusCode(500, "An error occurred while retrieving the comment");
        }
    }

    /// <summary>
    /// Create a new comment
    /// </summary>
    /// <param name="createCommentDto">Comment data</param>
    /// <returns>Created comment</returns>
    [HttpPost]
    public async Task<ActionResult<CommentDto>> CreateComment([FromBody] CreateCommentDto createCommentDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var comment = await _commentService.CreateCommentAsync(createCommentDto);
            
            return CreatedAtAction(
                nameof(GetComment), 
                new { commentId = comment.Id }, 
                comment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating comment for document {DocumentId}", createCommentDto.DocumentId);
            return StatusCode(500, "An error occurred while creating the comment");
        }
    }

    /// <summary>
    /// Update an existing comment
    /// </summary>
    /// <param name="commentId">Comment ID</param>
    /// <param name="updateCommentDto">Updated comment data</param>
    /// <returns>Updated comment</returns>
    [HttpPut("{commentId:int}")]
    public async Task<ActionResult<CommentDto>> UpdateComment(
        int commentId, 
        [FromBody] UpdateCommentDto updateCommentDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var comment = await _commentService.UpdateCommentAsync(commentId, updateCommentDto);
            
            if (comment == null)
            {
                return NotFound($"Comment with ID {commentId} not found");
            }

            return Ok(comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating comment {CommentId}", commentId);
            return StatusCode(500, "An error occurred while updating the comment");
        }
    }

    /// <summary>
    /// Delete a comment
    /// </summary>
    /// <param name="commentId">Comment ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{commentId:int}")]
    public async Task<ActionResult> DeleteComment(int commentId)
    {
        try
        {
            var success = await _commentService.DeleteCommentAsync(commentId);
            
            if (!success)
            {
                return NotFound($"Comment with ID {commentId} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
            return StatusCode(500, "An error occurred while deleting the comment");
        }
    }

    /// <summary>
    /// Get replies to a specific comment
    /// </summary>
    /// <param name="commentId">Parent comment ID</param>
    /// <returns>List of reply comments</returns>
    [HttpGet("{commentId:int}/replies")]
    public async Task<ActionResult<List<CommentDto>>> GetCommentReplies(int commentId)
    {
        try
        {
            var replies = await _commentService.GetCommentRepliesAsync(commentId);
            return Ok(replies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting replies for comment {CommentId}", commentId);
            return StatusCode(500, "An error occurred while retrieving replies");
        }
    }

    /// <summary>
    /// Get recent comments across all documents
    /// </summary>
    /// <param name="limit">Maximum number of comments to return (default: 20, max: 100)</param>
    /// <returns>List of recent comments</returns>
    [HttpGet("recent")]
    public async Task<ActionResult<List<CommentDto>>> GetRecentComments([FromQuery] int limit = 20)
    {
        try
        {
            if (limit < 1 || limit > 100)
            {
                limit = 20;
            }

            var comments = await _commentService.GetRecentCommentsAsync(limit);
            return Ok(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent comments");
            return StatusCode(500, "An error occurred while retrieving recent comments");
        }
    }

    /// <summary>
    /// Get comment statistics for a document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <returns>Comment statistics</returns>
    [HttpGet("document/{documentId:int}/statistics")]
    public async Task<ActionResult<object>> GetCommentStatistics(int documentId)
    {
        try
        {
            var statistics = await _commentService.GetCommentStatisticsAsync(documentId);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comment statistics for document {DocumentId}", documentId);
            return StatusCode(500, "An error occurred while retrieving comment statistics");
        }
    }
}
