using Nest;

namespace PaperlessRESTAPI.Models.Search;

/// <summary>
/// Document index model for Elasticsearch
/// </summary>
[ElasticsearchType(IdProperty = nameof(Id))]
public class DocumentIndex
{
    /// <summary>
    /// Document ID
    /// </summary>
    [Number(DocValues = false)]
    public int Id { get; set; }

    /// <summary>
    /// Document title
    /// </summary>
    [Text(Analyzer = "standard")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Original file name
    /// </summary>
    [Text(Analyzer = "standard")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File type/extension
    /// </summary>
    [Keyword]
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// OCR extracted text content
    /// </summary>
    [Text(Analyzer = "standard")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// AI-generated summary
    /// </summary>
    [Text(Analyzer = "standard")]
    public string? Summary { get; set; }

    /// <summary>
    /// Associated tags
    /// </summary>
    [Keyword]
    public List<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// Upload date
    /// </summary>
    [Date]
    public DateTime UploadDate { get; set; }

    /// <summary>
    /// Last modified date
    /// </summary>
    [Date]
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    [Number]
    public long FileSize { get; set; }

    /// <summary>
    /// Whether the document is processed
    /// </summary>
    [Boolean]
    public bool IsProcessed { get; set; }

    /// <summary>
    /// Processing confidence score
    /// </summary>
    [Number]
    public float? Confidence { get; set; }

    /// <summary>
    /// Whether AI processing is completed
    /// </summary>
    [Boolean]
    public bool IsAIProcessed { get; set; }
}
