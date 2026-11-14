using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebsiteMedical.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;

        // Trạng thái kết quả trả về từ API backend
        public bool IsSuccess { get; set; } = false;
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public LoginApiResponse? ResponseData { get; set; }
        public string? FormattedJson { get; set; }

        // Thông tin giải mã token JWT hoặc từ API
        public string? UserFullName { get; set; }
        public string? UserRole { get; set; }
        public string? UserEmail { get; set; }
        public string? UserExpires { get; set; }
    }

    public class LoginApiResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public LoginTokenData? Data { get; set; }
    }

    public class LoginTokenData
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("refreshExpiresAt")]
        public string RefreshExpiresAt { get; set; } = string.Empty;

        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }
}

