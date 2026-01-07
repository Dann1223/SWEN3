using PaperlessRESTAPI.Models.DTOs.Comments;

namespace PaperlessRESTAPI.Services.Interfaces;

/// <summary>
/// Service interface for document comments
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// Get all comments for a document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <param name="includeReplies">Whether to include nested replies</param>
    /// <returns>List of comments</returns>
    Task<List<CommentDto>> GetDocumentCommentsAsync(int documentId, bool includeReplies = true);

    /// <summary>
    /// Get a specific comment by ID
    /// </summary>
    /// <param name="commentId">Comment ID</param>
    /// <param name="includeReplies">Whether to include nested replies</param>
    /// <returns>Comment or null if not found</returns>
    Task<CommentDto?> GetCommentAsync(int commentId, bool includeReplies = true);

    /// <summary>
    /// Create a new comment
    /// </summary>
    /// <param name="createCommentDto">Comment data</param>
    /// <returns>Created comment</returns>
    Task<CommentDto> CreateCommentAsync(CreateCommentDto createCommentDto);

    /// <summary>
    /// Update an existing comment
    /// </summary>
    /// <param name="commentId">Comment ID</param>
    /// <param name="updateCommentDto">Updated comment data</param>
    /// <returns>Updated comment or null if not found</returns>
    Task<CommentDto?> UpdateCommentAsync(int commentId, UpdateCommentDto updateCommentDto);

    /// <summary>
    /// Delete a comment
    /// </summary>
    /// <param name="commentId">Comment ID</param>
    /// <returns>Success status</returns>
    Task<bool> DeleteCommentAsync(int commentId);

    /// <summary>
    /// Get replies to a specific comment
    /// </summary>
    /// <param name="parentCommentId">Parent comment ID</param>
    /// <returns>List of reply comments</returns>
    Task<List<CommentDto>> GetCommentRepliesAsync(int parentCommentId);

    /// <summary>
    /// Get recent comments across all documents
    /// </summary>
    /// <param name="limit">Maximum number of comments to return</param>
    /// <returns>List of recent comments</returns>
    Task<List<CommentDto>> GetRecentCommentsAsync(int limit = 20);

    /// <summary>
    /// Get comment statistics for a document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <returns>Comment statistics</returns>
    Task<object> GetCommentStatisticsAsync(int documentId);
}
