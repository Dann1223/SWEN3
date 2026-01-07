using System.Threading.Tasks;
using PaperlessRESTAPI.Models;

namespace PaperlessRESTAPI.Services.Interfaces
{
    public interface IGenAIResultService
    {
        /// <summary>
        /// Processes a GenAI result and updates the document
        /// </summary>
        /// <param name="result">The GenAI result to process</param>
        /// <returns>True if processing was successful</returns>
        Task<bool> ProcessGenAIResultAsync(GenAIResultMessage result);
        
        /// <summary>
        /// Gets the AI processing status for a document
        /// </summary>
        /// <param name="documentId">The document ID</param>
        /// <returns>Processing status information</returns>
        Task<(bool IsProcessed, string Summary, List<string> SuggestedTags)> GetAIProcessingStatusAsync(int documentId);
    }
}
