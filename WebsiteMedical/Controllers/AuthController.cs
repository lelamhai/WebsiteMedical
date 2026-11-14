using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebsiteMedical.Models;

namespace WebsiteMedical.Controllers
{
    public class AuthController : Controller
    {
        public const string BASE_URL = "https://localhost:7182";
        private readonly string LOGIN = $"{BASE_URL}/api/Auth/login";

        [HttpGet]
        [Route("")]
        [Route("login")]
        [Route("Auth/Login")]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [Route("")]
        [Route("login")]
        [Route("Auth/Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // 2. Khởi tạo HttpClient trực tiếp (chấp nhận SSL dev trên localhost)
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                using var client = new HttpClient(handler);

                // 3. Chuẩn bị dữ liệu gửi lên API: { "email": "...", "password": "..." }
                var payload = new
                {
                    email = model.Email.Trim(),
                    password = model.Password
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                // 4. Gửi yêu cầu đăng nhập đến link LOGIN
                var response = await client.PostAsync(LOGIN, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var apiResult = JsonSerializer.Deserialize<LoginApiResponse>(responseContent, options);

                    model.IsSuccess = true;
                    model.ResponseData = apiResult;

                    // Định dạng JSON để hiển thị màn hình kết quả giống trong hình
                    using var doc = JsonDocument.Parse(responseContent);
                    model.FormattedJson = JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });

                    // Lưu Cookie phiên đăng nhập
                    if (apiResult?.Data?.AccessToken != null)
                    {
                        Response.Cookies.Append("AccessToken", apiResult.Data.AccessToken, new CookieOptions
                        {
                            HttpOnly = false,
                            Secure = true,
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTimeOffset.UtcNow.AddHours(2)
                        });

                        ExtractJwtClaims(apiResult.Data.AccessToken, model);

                        // Lưu Cookie Họ và Tên (URL Encode để an toàn với ký tự tiếng Việt có dấu)
                        var displayName = !string.IsNullOrWhiteSpace(model.UserFullName) 
                            ? model.UserFullName 
                            : (!string.IsNullOrWhiteSpace(model.UserEmail) ? model.UserEmail.Split('@')[0] : "Người dùng");

                        Response.Cookies.Append("UserName", System.Net.WebUtility.UrlEncode(displayName), new CookieOptions
                        {
                            HttpOnly = false,
                            Secure = true,
                            Expires = DateTimeOffset.UtcNow.AddHours(2)
                        });

                        Response.Cookies.Append("UserRole", model.UserRole ?? "Admin", new CookieOptions
                        {
                            HttpOnly = false,
                            Secure = true,
                            Expires = DateTimeOffset.UtcNow.AddHours(2)
                        });

                        // Điều hướng người dùng theo 3 vai trò: Admin, Staff, Volunteer
                        if (string.Equals(model.UserRole, "Admin", StringComparison.OrdinalIgnoreCase))
                        {
                            return RedirectToAction("Campaigns", "Admin");
                        }
                        else if (string.Equals(model.UserRole, "Staff", StringComparison.OrdinalIgnoreCase))
                        {
                            return RedirectToAction("DutySchedule", "Staff");
                        }
                        else if (string.Equals(model.UserRole, "Volunteer", StringComparison.OrdinalIgnoreCase))
                        {
                            return RedirectToAction("AccessDenied", "Auth");
                        }
                    }
                }
                else
                {
                    model.IsSuccess = false;
                    model.ErrorMessage = "Email hoặc mật khẩu không chính xác. Vui lòng kiểm tra lại.";
                }
            }
            catch (Exception)
            {
                model.IsSuccess = false;
                model.ErrorMessage = $"Không thể kết nối đến máy chủ API ({BASE_URL}). Vui lòng đảm bảo backend đang chạy.";
            }

            return View(model);
        }

        [HttpGet]
        [Route("access-denied")]
        [Route("Auth/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Route("logout")]
        [Route("Auth/Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("UserName");
            Response.Cookies.Delete("UserRole");
            return RedirectToAction("Login", "Auth");
        }

        /// <summary>
        /// Giải mã JWT Token để lấy vai trò và thông tin họ tên người dùng
        /// </summary>
        private void ExtractJwtClaims(string token, LoginViewModel model)
        {
            // 1. Kiểm tra nếu backend API trả về trực tiếp trong response data
            if (!string.IsNullOrWhiteSpace(model.ResponseData?.Data?.FullName))
            {
                model.UserFullName = model.ResponseData.Data.FullName;
            }
            else if (!string.IsNullOrWhiteSpace(model.ResponseData?.Data?.Name))
            {
                model.UserFullName = model.ResponseData.Data.Name;
            }
            else if (!string.IsNullOrWhiteSpace(model.ResponseData?.Data?.UserName))
            {
                model.UserFullName = model.ResponseData.Data.UserName;
            }

            try
            {
                var parts = token.Split('.');
                if (parts.Length >= 2)
                {
                    var payload = parts[1].Replace('-', '+').Replace('_', '/');
                    switch (payload.Length % 4)
                    {
                        case 2: payload += "=="; break;
                        case 3: payload += "="; break;
                    }

                    var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                    using var doc = JsonDocument.Parse(json);

                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        var propName = prop.Name;
                        if (propName.Equals("role", StringComparison.OrdinalIgnoreCase) || propName.EndsWith("/role"))
                        {
                            model.UserRole = prop.Value.GetString();
                        }
                        else if (propName.Equals("email", StringComparison.OrdinalIgnoreCase) || propName.EndsWith("/emailaddress"))
                        {
                            model.UserEmail = prop.Value.GetString();
                        }
                        else if (string.IsNullOrEmpty(model.UserFullName))
                        {
                            if (propName.Equals("fullname", StringComparison.OrdinalIgnoreCase) ||
                                propName.Equals("fullName", StringComparison.OrdinalIgnoreCase) ||
                                propName.Equals("name", StringComparison.OrdinalIgnoreCase) ||
                                propName.EndsWith("/name") ||
                                propName.Equals("unique_name", StringComparison.OrdinalIgnoreCase) ||
                                propName.Equals("username", StringComparison.OrdinalIgnoreCase) ||
                                propName.Equals("given_name", StringComparison.OrdinalIgnoreCase))
                            {
                                model.UserFullName = prop.Value.GetString();
                            }
                        }
                    }
                }
            }
            catch { }

            if (string.IsNullOrEmpty(model.UserRole)) model.UserRole = "Admin";
            if (string.IsNullOrEmpty(model.UserEmail)) model.UserEmail = model.Email;

            // Nếu không có tên trong JWT, tự động suy ra từ email hoặc mặc định theo role
            if (string.IsNullOrWhiteSpace(model.UserFullName))
            {
                if (!string.IsNullOrWhiteSpace(model.UserEmail) && model.UserEmail.Contains('@'))
                {
                    var emailUser = model.UserEmail.Split('@')[0];
                    model.UserFullName = emailUser;
                }
                else
                {
                    model.UserFullName = model.UserRole.Equals("Staff", StringComparison.OrdinalIgnoreCase) 
                        ? "Bác sĩ Trực ban" 
                        : "Quản Trị Viên";
                }
            }

            if (model.ResponseData?.Data?.RefreshExpiresAt != null &&
                DateTime.TryParse(model.ResponseData.Data.RefreshExpiresAt, out var exp))
            {
                model.UserExpires = exp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
            }
            else
            {
                model.UserExpires = "20/11/2026 22:52:30";
            }
        }
    }
}
