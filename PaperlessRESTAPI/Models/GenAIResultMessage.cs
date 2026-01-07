using System;
using System.Collections.Generic;

namespace PaperlessRESTAPI.Models
{
    public class GenAIResultMessage
    {
        public int DocumentId { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<string> SuggestedTags { get; set; } = new List<string>();
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
