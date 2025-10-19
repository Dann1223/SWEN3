namespace PaperlessServices.Configuration;

public class MinIOConfig
{
    public string Endpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public bool UseSSL { get; set; } = false;
}

public class TesseractConfig
{
    public string DataPath { get; set; } = "./tessdata";
    public string DefaultLanguage { get; set; } = "eng";
}

public class RabbitMQConfig
{
    public string HostName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int Port { get; set; } = 5672;
    public string OcrQueue { get; set; } = "ocr.queue";
    public string ResultQueue { get; set; } = "result.queue";
}
