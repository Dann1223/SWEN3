using System;
using System.Threading.Tasks;

namespace PaperlessServices.Services.Interfaces
{
    public interface IGenAIService
    {
        /// <summary>
        /// Generates a summary for the given text using Google Gemini API
        /// </summary>
        /// <param name="text">The text to summarize</param>
        /// <param name="maxLength">Maximum length of the summary (optional)</param>
        /// <returns>The generated summary</returns>
        Task<string> GenerateSummaryAsync(string text, int maxLength = 200);
        
        /// <summary>
        /// Checks if the GenAI service is available
        /// </summary>
        /// <returns>True if the service is available</returns>
        Task<bool> IsAvailableAsync();
        
        /// <summary>
        /// Extracts key topics and tags from the given text
        /// </summary>
        /// <param name="text">The text to analyze</param>
        /// <returns>List of suggested tags</returns>
        Task<List<string>> ExtractTagsAsync(string text);
    }
}
