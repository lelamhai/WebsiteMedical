using System.Net.Http.Headers;
using System.Text.Json;
using WebsiteMedical.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebsiteMedical.Controllers
{
    public class StaffController : Controller
    {
        public const string BASE_URL = "https://localhost:7182";
        private readonly string STAFF_SCHEDULE_API = $"{BASE_URL}/api/Staff/schedule";
        private readonly string CAMPAIGN_API = $"{BASE_URL}/api/Campaign/list";
        private readonly string HEALTH_SCREENING_API = $"{BASE_URL}/api/healthscreening/list";
        private readonly string USER_API = $"{BASE_URL}/api/User";
        private readonly string HEALTH_SCREENING_UPDATE_API = $"{BASE_URL}/api/HealthScreening/update";

        [HttpGet]
        [Route("Staff")]
        public IActionResult Index()
        {
            return RedirectToAction("DutySchedule");
        }

        // Menu 1: Xem phân công trực của Campaigns
        [HttpGet]
        [Route("Staff/DutySchedule")]
        public async Task<IActionResult> DutySchedule(int page = 1)
        {
            page = Math.Max(page, 1);
            var model = new StaffScheduleViewModel { Page = page };

            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                using var client = new HttpClient(handler);

                string? token = null;
                if (Request.Headers.TryGetValue("Authorization", out var authHeader)
                    && !string.IsNullOrEmpty(authHeader))
                {
                    var authStr = authHeader.ToString().Trim();
                    token = authStr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? authStr.Substring(7).Trim()
                        : authStr;
                }
                if (string.IsNullOrEmpty(token))
                {
                    token = Request.Cookies["AccessToken"];
                }

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync($"{STAFF_SCHEDULE_API}?page={page}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var apiResponse = JsonSerializer.Deserialize<StaffScheduleResponse>(responseContent, options);

                    if (apiResponse?.Data != null)
                    {
                        model.Schedules = apiResponse.Data.Items ?? [];
                        model.Page = apiResponse.Data.Page > 0 ? apiResponse.Data.Page : page;
                        model.PageSize = apiResponse.Data.PageSize;
                        model.TotalItems = apiResponse.Data.TotalItems;
                        model.TotalPages = apiResponse.Data.TotalPages;
                    }
                    else
                    {
                        model.ErrorMessage = apiResponse?.Message ?? "Không có dữ liệu lịch trực trả về từ API.";
                    }
                }
                else
                {
                    model.ErrorMessage = $"Không thể tải lịch trực (HTTP {(int)response.StatusCode}).";
                }
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage = $"Không thể kết nối đến {STAFF_SCHEDULE_API}. Vui lòng đảm bảo backend đang chạy.";
            }
            catch (TaskCanceledException)
            {
                model.ErrorMessage = "Yêu cầu tải lịch trực đã hết thời gian chờ. Vui lòng thử lại sau.";
            }

            return View(model);
        }

        // Menu: Tạo hồ sơ khám
        [HttpGet]
        [Route("Staff/CreateRecord")]
        public IActionResult CreateRecord()
        {
            return View();
        }

        // Menu 2: Quản lý hồ sơ khám
        [HttpGet]
        [Route("Staff/MedicalRecords")]
        public async Task<IActionResult> MedicalRecords()
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                using var client = new HttpClient(handler);

                string? token = null;
                if (Request.Headers.TryGetValue("Authorization", out var authHeader)
                    && !string.IsNullOrEmpty(authHeader))
                {
                    var authStr = authHeader.ToString().Trim();
                    token = authStr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? authStr.Substring(7).Trim()
                        : authStr;
                }
                if (string.IsNullOrEmpty(token))
                {
                    token = Request.Cookies["AccessToken"];
                }

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync(CAMPAIGN_API);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var apiResponse = JsonSerializer.Deserialize<CampaignApiResponse>(responseContent, options);

                    if (apiResponse?.Data?.Items != null)
                    {
                        ViewBag.Campaigns = apiResponse.Data.Items;
                    }
                    else
                    {
                        ViewBag.ErrorMessage = apiResponse?.Message ?? "Không có dữ liệu chiến dịch";
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = $"Không thể tải chiến dịch (HTTP {(int)response.StatusCode}).";
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = $"Không thể kết nối đến {CAMPAIGN_API}. Vui lòng đảm bảo backend đang chạy.";
            }
            catch (TaskCanceledException)
            {
                ViewBag.ErrorMessage = "Yêu cầu tải chiến dịch đã hết thời gian chờ. Vui lòng thử lại sau.";
            }

            return View();
        }

        // API Proxy: GET health screening list (tránh CORS)
        [HttpGet]
        [Route("api/staff/health-screenings")]
        public async Task<IActionResult> GetHealthScreenings(int campaignId)
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                using var client = new HttpClient(handler);

                // Lấy token từ header hoặc cookie
                string? token = null;
                if (Request.Headers.TryGetValue("Authorization", out var authHeader)
                    && !string.IsNullOrEmpty(authHeader))
                {
                    var authStr = authHeader.ToString().Trim();
                    token = authStr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? authStr.Substring(7).Trim()
                        : authStr;
                }
                if (string.IsNullOrEmpty(token))
                {
                    token = Request.Cookies["AccessToken"];
                }

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                // Gọi API backend
                var apiUrl = $"{HEALTH_SCREENING_API}?campaignId={campaignId}";
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return Content(responseContent, "application/json");
                }
                else
                {
                    return StatusCode((int)response.StatusCode, new
                    {
                        success = false,
                        message = $"API Backend trả về lỗi: {response.StatusCode}"
                    });
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Không thể kết nối đến API Backend: {ex.Message}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi tải dữ liệu: {ex.Message}"
                });
            }
        }

        // API Proxy: PUT health screening result (same-origin endpoint, avoids browser CORS)
        [HttpPut]
        [Route("api/staff/health-screenings/{screeningId:int}")]
        public async Task<IActionResult> UpdateHealthScreening(
            int screeningId,
            [FromBody] UpdateHealthScreeningRequest request)
        {
            var screeningResult = request.ScreeningResult?.Trim();
            if (screeningResult is not ("Passed" or "Failed"))
            {
                return BadRequest(new { message = "screeningResult must be Passed or Failed." });
            }

            var rejectionReason = screeningResult == "Passed"
                ? string.Empty
                : request.RejectionReason?.Trim() ?? string.Empty;

            if (screeningResult == "Failed" && string.IsNullOrWhiteSpace(rejectionReason))
            {
                return BadRequest(new { message = "Vui lòng nhập hoặc chọn lý do không đạt." });
            }

            try
            {
                using var client = new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

                var token = Request.Cookies["AccessToken"];
                if (string.IsNullOrWhiteSpace(token)
                    && Request.Headers.TryGetValue("Authorization", out var authHeader)
                    && !string.IsNullOrWhiteSpace(authHeader))
                {
                    var authValue = authHeader.ToString().Trim();
                    token = authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? authValue[7..].Trim()
                        : authValue;
                }

                if (!string.IsNullOrWhiteSpace(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var payload = new { screeningResult, rejectionReason };
                using var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await client.PutAsync(
                    $"{HEALTH_SCREENING_UPDATE_API}/{screeningId}",
                    content);
                var responseContent = await response.Content.ReadAsStringAsync();

                return new ContentResult
                {
                    StatusCode = (int)response.StatusCode,
                    Content = responseContent,
                    ContentType = "application/json"
                };
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new { message = $"Không thể kết nối đến API Backend: {ex.Message}" });
            }
        }

        // API Proxy: GET user details (same-origin endpoint, avoids browser CORS)
        [HttpGet]
        [Route("api/staff/users/{userId:int}")]
        public async Task<IActionResult> GetUser(int userId)
        {
            try
            {
                using var client = new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

                // Match the other same-origin API proxies: use the login cookie first.
                // A stale localStorage bearer header should not override this token.
                var token = Request.Cookies["AccessToken"];
                if (string.IsNullOrWhiteSpace(token)
                    && Request.Headers.TryGetValue("Authorization", out var authHeader)
                    && !string.IsNullOrWhiteSpace(authHeader))
                {
                    var authValue = authHeader.ToString().Trim();
                    token = authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? authValue[7..].Trim()
                        : authValue;
                }
                if (!string.IsNullOrWhiteSpace(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync($"{USER_API}/{userId}");
                var responseContent = await response.Content.ReadAsStringAsync();
                return new ContentResult
                {
                    StatusCode = (int)response.StatusCode,
                    Content = responseContent,
                    ContentType = "application/json"
                };
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new
                {
                    success = false,
                    message = $"Không thể kết nối đến API Backend: {ex.Message}"
                });
            }
        }
    }

    public sealed class UpdateHealthScreeningRequest
    {
        public string? ScreeningResult { get; set; }
        public string? RejectionReason { get; set; }
    }
}

