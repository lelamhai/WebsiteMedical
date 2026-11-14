using System.Text.Json.Serialization;

namespace WebsiteMedical.Models
{
    public class StaffScheduleViewModel
    {
        public IReadOnlyList<StaffScheduleItem> Schedules { get; set; } = [];
        public string? ErrorMessage { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class StaffScheduleResponse
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public StaffSchedulePageData? Data { get; set; }
    }

    public class StaffSchedulePageData
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("totalItems")]
        public int TotalItems { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("items")]
        public List<StaffScheduleItem>? Items { get; set; }
    }

    public class StaffScheduleItem
    {
        [JsonPropertyName("campaignId")]
        public int CampaignId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("eventDate")]
        public DateTime EventDate { get; set; }

        [JsonPropertyName("shiftTime")]
        public string ShiftTime { get; set; } = string.Empty;
    }
}
