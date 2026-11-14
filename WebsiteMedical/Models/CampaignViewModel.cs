using System.Text.Json.Serialization;

namespace WebsiteMedical.Models
{
    public class CampaignApiResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public CampaignPagedData? Data { get; set; }
    }

    public class CampaignPagedData
    {
        [JsonPropertyName("page")]
        public int Page { get; set; } = 1;

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; } = 10;

        [JsonPropertyName("totalItems")]
        public int TotalItems { get; set; } = 0;

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; } = 1;

        [JsonPropertyName("items")]
        public List<CampaignItem> Items { get; set; } = new();
    }

    public class CampaignItem
    {
        [JsonPropertyName("campaignId")]
        public int CampaignId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("eventDate")]
        public DateTime? EventDate { get; set; }

        [JsonPropertyName("adminId")]
        public int AdminId { get; set; }

        [JsonPropertyName("staffCount")]
        public int StaffCount { get; set; }

        [JsonPropertyName("staffAssignments")]
        public List<object>? StaffAssignments { get; set; }

        // Helper properties for display
        [JsonIgnore]
        public string FormattedEventDate => EventDate.HasValue 
            ? EventDate.Value.ToString("dd/MM/yyyy HH:mm") 
            : "Chưa xác định";

        [JsonIgnore]
        public string StatusBadgeClass
        {
            get
            {
                if (!EventDate.HasValue) return "badge-upcoming";
                if (EventDate.Value.Date == DateTime.Today) return "badge-active";
                if (EventDate.Value > DateTime.Now) return "badge-upcoming";
                return "badge-ended";
            }
        }

        [JsonIgnore]
        public string StatusText
        {
            get
            {
                if (!EventDate.HasValue) return "Chờ xếp lịch";
                if (EventDate.Value.Date == DateTime.Today) return "Đang diễn ra";
                if (EventDate.Value > DateTime.Now) return "Sắp diễn ra";
                return "Đã kết thúc";
            }
        }
    }

    public class CampaignListViewModel
    {
        public CampaignPagedData PagedData { get; set; } = new();
        public bool IsApiSuccess { get; set; } = true;
        public string? ErrorMessage { get; set; }
        public bool IsUsingFallback { get; set; } = false;
    }

    // DTO gửi lên API POST /api/Campaign/create
    public class CreateCampaignRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("eventDate")]
        public DateTime EventDate { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("staffAssignments")]
        public List<StaffAssignmentRequest> StaffAssignments { get; set; } = new();
    }

    public class StaffAssignmentRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("staffId")]
        public int StaffId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("shiftTime")]
        public string ShiftTime { get; set; } = string.Empty;
    }
}

