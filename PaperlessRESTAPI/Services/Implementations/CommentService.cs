using Microsoft.EntityFrameworkCore;
using PaperlessRESTAPI.Data;
using PaperlessRESTAPI.Data.Entities;
using PaperlessRESTAPI.Models.DTOs.Comments;
using PaperlessRESTAPI.Services.Interfaces;

namespace PaperlessRESTAPI.Services.Implementations;

/// <summary>
/// Service implementation for document comments
/// </summary>
public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CommentService> _logger;

    public CommentService(ApplicationDbContext context, ILogger<CommentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<CommentDto>> GetDocumentCommentsAsync(int documentId, bool includeReplies = true)
    {
        try
        {
            var query = _context.DocumentComments
                .Where(c => c.DocumentId == documentId && c.ParentCommentId == null);

            if (includeReplies)
            {
                query = query.Include(c => c.Replies);
            }

            var comments = await query.OrderBy(c => c.CreatedAt).ToListAsync();

            return comments.Select(c => MapToCommentDto(c, includeReplies)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments for document {DocumentId}", documentId);
            return new List<CommentDto>();
        }
    }

    public async Task<CommentDto?> GetCommentAsync(int commentId, bool includeReplies = true)
    {
        try
        {
            var query = _context.DocumentComments.Where(c => c.Id == commentId);

            if (includeReplies)
            {
                query = query.Include(c => c.Replies);
            }

            var comment = await query.FirstOrDefaultAsync();

            return comment != null ? MapToCommentDto(comment, includeReplies) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comment {CommentId}", commentId);
            return null;
        }
    }

    public async Task<CommentDto> CreateCommentAsync(CreateCommentDto createCommentDto)
    {
        try
        {
            // Verify document exists
            var documentExists = await _context.Documents
                .AnyAsync(d => d.Id == createCommentDto.DocumentId);

            if (!documentExists)
            {
                throw new ArgumentException($"Document with ID {createCommentDto.DocumentId} not found");
            }

            // Verify parent comment exists (if specified)
            if (createCommentDto.ParentCommentId.HasValue)
            {
                var parentExists = await _context.DocumentComments
                    .AnyAsync(c => c.Id == createCommentDto.ParentCommentId.Value && 
                                   c.DocumentId == createCommentDto.DocumentId);

                if (!parentExists)
                {
                    throw new ArgumentException($"Parent comment with ID {createCommentDto.ParentCommentId} not found or belongs to different document");
                }
            }

            var comment = new DocumentComment
            {
                DocumentId = createCommentDto.DocumentId,
                AuthorName = createCommentDto.AuthorName,
                Content = createCommentDto.Content,
                ParentCommentId = createCommentDto.ParentCommentId,
                Position = createCommentDto.Position,
                CreatedAt = DateTime.UtcNow
            };

            _context.DocumentComments.Add(comment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created comment {CommentId} for document {DocumentId} by {AuthorName}", 
                comment.Id, createCommentDto.DocumentId, createCommentDto.AuthorName);

            return MapToCommentDto(comment, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating comment for document {DocumentId}", createCommentDto.DocumentId);
            throw;
        }
    }

    public async Task<CommentDto?> UpdateCommentAsync(int commentId, UpdateCommentDto updateCommentDto)
    {
        try
        {
            var comment = await _context.DocumentComments
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                return null;
            }

            comment.Content = updateCommentDto.Content;
            comment.UpdatedAt = DateTime.UtcNow;
            comment.IsEdited = true;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated comment {CommentId}", commentId);

            return MapToCommentDto(comment, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating comment {CommentId}", commentId);
            throw;
        }
    }

    public async Task<bool> DeleteCommentAsync(int commentId)
    {
        try
        {
            var comment = await _context.DocumentComments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                return false;
            }

            // Delete all replies first
            if (comment.Replies.Any())
            {
                _context.DocumentComments.RemoveRange(comment.Replies);
            }

            _context.DocumentComments.Remove(comment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted comment {CommentId} and {ReplyCount} replies", 
                commentId, comment.Replies.Count);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
            return false;
        }
    }

    public async Task<List<CommentDto>> GetCommentRepliesAsync(int parentCommentId)
    {
        try
        {
            var replies = await _context.DocumentComments
                .Where(c => c.ParentCommentId == parentCommentId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return replies.Select(r => MapToCommentDto(r, false)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting replies for comment {ParentCommentId}", parentCommentId);
            return new List<CommentDto>();
        }
    }

    public async Task<List<CommentDto>> GetRecentCommentsAsync(int limit = 20)
    {
        try
        {
            var comments = await _context.DocumentComments
                .Include(c => c.Document)
                .OrderByDescending(c => c.CreatedAt)
                .Take(limit)
                .ToListAsync();

            return comments.Select(c => MapToCommentDto(c, false)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent comments");
            return new List<CommentDto>();
        }
    }

    public async Task<object> GetCommentStatisticsAsync(int documentId)
    {
        try
        {
            var stats = await _context.DocumentComments
                .Where(c => c.DocumentId == documentId)
                .GroupBy(c => c.DocumentId)
                .Select(g => new
                {
                    DocumentId = g.Key,
                    TotalComments = g.Count(),
                    TopLevelComments = g.Count(c => c.ParentCommentId == null),
                    Replies = g.Count(c => c.ParentCommentId != null),
                    UniqueAuthors = g.Select(c => c.AuthorName).Distinct().Count(),
                    LatestComment = g.Max(c => c.CreatedAt),
                    AuthorBreakdown = g.GroupBy(c => c.AuthorName)
                        .Select(ag => new { Author = ag.Key, Count = ag.Count() })
                        .OrderByDescending(ag => ag.Count)
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (stats != null)
            {
                return new
                {
                    DocumentId = stats.DocumentId,
                    TotalComments = stats.TotalComments,
                    TopLevelComments = stats.TopLevelComments,
                    Replies = stats.Replies,
                    UniqueAuthors = stats.UniqueAuthors,
                    LatestComment = (DateTime?)stats.LatestComment,
                    AuthorBreakdown = stats.AuthorBreakdown
                };
            }

            return new
            {
                DocumentId = documentId,
                TotalComments = 0,
                TopLevelComments = 0,
                Replies = 0,
                UniqueAuthors = 0,
                LatestComment = (DateTime?)null,
                AuthorBreakdown = new List<object>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comment statistics for document {DocumentId}", documentId);
            return new
            {
                DocumentId = documentId,
                TotalComments = 0,
                TopLevelComments = 0,
                Replies = 0,
                UniqueAuthors = 0,
                LatestComment = (DateTime?)null,
                AuthorBreakdown = new List<object>(),
                Error = "Failed to retrieve statistics"
            };
        }
    }

    #region Private Helper Methods

    private CommentDto MapToCommentDto(DocumentComment comment, bool includeReplies)
    {
        var dto = new CommentDto
        {
            Id = comment.Id,
            DocumentId = comment.DocumentId,
            AuthorName = comment.AuthorName,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            ParentCommentId = comment.ParentCommentId,
            Position = comment.Position,
            IsEdited = comment.IsEdited,
            ReplyCount = comment.Replies?.Count ?? 0
        };

        if (includeReplies && comment.Replies?.Any() == true)
        {
            dto.Replies = comment.Replies
                .OrderBy(r => r.CreatedAt)
                .Select(r => MapToCommentDto(r, false))
                .ToList();
        }

        return dto;
    }

    #endregion
}
