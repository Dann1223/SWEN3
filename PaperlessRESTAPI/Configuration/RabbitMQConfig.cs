namespace PaperlessRESTAPI.Configuration;

public class RabbitMQConfig
{
    public string HostName { get; set; } = "localhost";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    
    // Queue Names
    public string OcrQueue { get; set; } = "ocr.queue";
    public string GenAIQueue { get; set; } = "genai.queue";
    public string IndexingQueue { get; set; } = "indexing.queue";
    public string ResultQueue { get; set; } = "result.queue";
    
    // Exchange Names
    public string DocumentExchange { get; set; } = "document.exchange";
    public string ProcessingExchange { get; set; } = "processing.exchange";
}
