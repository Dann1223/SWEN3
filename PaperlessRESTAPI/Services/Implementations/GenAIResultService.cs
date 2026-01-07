using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PaperlessRESTAPI.Data.Entities;
using PaperlessRESTAPI.Data.Repositories;
using PaperlessRESTAPI.Models;
using PaperlessRESTAPI.Services.Interfaces;

namespace PaperlessRESTAPI.Services.Implementations
{
    public class GenAIResultService : IGenAIResultService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GenAIResultService> _logger;

        public GenAIResultService(IUnitOfWork unitOfWork, ILogger<GenAIResultService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> ProcessGenAIResultAsync(GenAIResultMessage result)
        {
            try
            {
                _logger.LogInformation("Processing GenAI result for document {DocumentId}", result.DocumentId);

                var document = await _unitOfWork.Documents.GetByIdAsync(result.DocumentId);
                if (document == null)
                {
                    _logger.LogWarning("Document {DocumentId} not found for GenAI result processing", result.DocumentId);
                    return false;
                }

                if (result.Success)
                {
                    // Update document with AI-generated summary
                    if (!string.IsNullOrWhiteSpace(result.Summary))
                    {
                        document.Summary = result.Summary;
                        _logger.LogInformation("Updated summary for document {DocumentId}, length: {Length}", 
                            result.DocumentId, result.Summary.Length);
                    }

                    // Add suggested tags if any
                    if (result.SuggestedTags != null && result.SuggestedTags.Any())
                    {
                        await AddSuggestedTags(document, result.SuggestedTags);
                    }

                    // Mark AI processing as completed
                    document.IsAIProcessed = true;
                    document.AIProcessedAt = result.ProcessedAt;
                }
                else
                {
                    _logger.LogWarning("GenAI processing failed for document {DocumentId}: {Error}", 
                        result.DocumentId, result.ErrorMessage);
                    
                    // Mark as processed but with error
                    document.IsAIProcessed = true;
                    document.AIProcessedAt = result.ProcessedAt;
                    document.AIErrorMessage = result.ErrorMessage;
                }

                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Successfully processed GenAI result for document {DocumentId}", result.DocumentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process GenAI result for document {DocumentId}", result.DocumentId);
                return false;
            }
        }

        public async Task<(bool IsProcessed, string Summary, List<string> SuggestedTags)> GetAIProcessingStatusAsync(int documentId)
        {
            try
            {
                var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
                if (document == null)
                {
                    return (false, string.Empty, new List<string>());
                }

                var suggestedTags = document.Tags?.Where(t => t.IsAIGenerated)
                    .Select(t => t.Name)
                    .ToList() ?? new List<string>();

                return (document.IsAIProcessed, document.Summary ?? string.Empty, suggestedTags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get AI processing status for document {DocumentId}", documentId);
                return (false, string.Empty, new List<string>());
            }
        }

        private async Task AddSuggestedTags(Document document, List<string> suggestedTagNames)
        {
            try
            {
                var existingTagNames = document.Tags?.Select(t => t.Name.ToLowerInvariant()).ToHashSet() 
                    ?? new HashSet<string>();

                foreach (var tagName in suggestedTagNames)
                {
                    if (string.IsNullOrWhiteSpace(tagName) || tagName.Length < 2)
                        continue;

                    var normalizedTagName = tagName.Trim().ToLowerInvariant();
                    
                    // Skip if tag already exists on document
                    if (existingTagNames.Contains(normalizedTagName))
                        continue;

                    // Find or create the tag
                    var existingTag = await _unitOfWork.Tags.FirstOrDefaultAsync(t => 
                        t.Name.ToLower() == normalizedTagName);

                    Tag tag;
                    if (existingTag != null)
                    {
                        tag = existingTag;
                    }
                    else
                    {
                        // Create new AI-generated tag
                        tag = new Tag
                        {
                            Name = tagName.Trim(),
                            Description = $"AI-generated tag",
                            Color = GetRandomTagColor(),
                            IsAIGenerated = true,
                            CreatedDate = DateTime.UtcNow
                        };
                        
                        await _unitOfWork.Tags.AddAsync(tag);
                    }

                    // Add tag to document
                    document.Tags ??= new List<Tag>();
                    document.Tags.Add(tag);
                    
                    existingTagNames.Add(normalizedTagName);
                    
                    _logger.LogDebug("Added suggested tag '{TagName}' to document {DocumentId}", 
                        tag.Name, document.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add suggested tags to document {DocumentId}", document.Id);
            }
        }

        private string GetRandomTagColor()
        {
            var colors = new[]
            {
                "#3B82F6", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6",
                "#06B6D4", "#84CC16", "#F97316", "#EC4899", "#6366F1"
            };
            
            var random = new Random();
            return colors[random.Next(colors.Length)];
        }
    }
}
