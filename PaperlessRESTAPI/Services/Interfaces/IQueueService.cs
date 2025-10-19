using PaperlessRESTAPI.Models.Messages;

namespace PaperlessRESTAPI.Services.Interfaces;

public interface IQueueService
{
    Task SendOcrMessageAsync(OcrMessage message);
    Task SendGenAIMessageAsync(GenAIMessage message);
    Task SendIndexingMessageAsync(IndexingMessage message);
    Task<bool> IsHealthyAsync();
    void Dispose();
}
