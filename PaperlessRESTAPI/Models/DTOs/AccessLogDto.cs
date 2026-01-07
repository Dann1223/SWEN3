using System.Xml.Serialization;

namespace PaperlessRESTAPI.Models.DTOs;

/// <summary>
/// Root XML structure for daily access logs
/// </summary>
[XmlRoot("AccessLogReport")]
public class AccessLogReportDto
{
    [XmlAttribute("version")]
    public string Version { get; set; } = "1.0";

    [XmlAttribute("date")]
    public string Date { get; set; } = string.Empty;

    [XmlAttribute("system")]
    public string System { get; set; } = string.Empty;

    [XmlElement("DocumentAccess")]
    public List<DocumentAccessDto> DocumentAccesses { get; set; } = new();
}

/// <summary>
/// Individual document access data from XML
/// </summary>
public class DocumentAccessDto
{
    [XmlAttribute("documentId")]
    public int DocumentId { get; set; }

    [XmlAttribute("fileName")]
    public string? FileName { get; set; }

    [XmlElement("ViewCount")]
    public int ViewCount { get; set; }

    [XmlElement("DownloadCount")]
    public int DownloadCount { get; set; }

    [XmlElement("SearchCount")]
    public int SearchCount { get; set; }

    [XmlElement("TotalAccess")]
    public int TotalAccess { get; set; }

    [XmlElement("AccessDetails")]
    public List<AccessDetailDto>? AccessDetails { get; set; }
}

/// <summary>
/// Detailed access information (optional)
/// </summary>
public class AccessDetailDto
{
    [XmlAttribute("time")]
    public string Time { get; set; } = string.Empty;

    [XmlAttribute("action")]
    public string Action { get; set; } = string.Empty;

    [XmlAttribute("userAgent")]
    public string? UserAgent { get; set; }

    [XmlAttribute("ipAddress")]
    public string? IpAddress { get; set; }
}
