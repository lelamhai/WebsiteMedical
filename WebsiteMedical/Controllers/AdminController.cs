using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebsiteMedical.Models;

namespace WebsiteMedical.Controllers
{
    public class AdminController : Controller
    {
        public const string BASE_URL = "https://localhost:7182";
        private readonly string CAMPAIGN_LIST_API = $"{BASE_URL}/api/Campaign/list";
        private readonly string CAMPAIGN_CREATE_API = $"{BASE_URL}/api/Campaign/create";
        private readonly string STAFF_SEARCH_API = $"{BASE_URL}/api/Staff/search";
        private readonly string CAMPAIGN_DELETE_API = $"{BASE_URL}/api/Campaign/delete";

        [HttpGet]
        [Route("Admin")]
        public IActionResult Index()
        {
            return RedirectToAction("Campaigns");
        }

        // Menu 1: Quản lý Campaigns (kèm phân trang từ API)
        [HttpGet]
        [Route("Admin/Campaigns")]
        public async Task<IActionResult> Campaigns(int page = 1)
        {
            if (page < 1) page = 1;

            var model = new CampaignListViewModel();

            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                using var client = new HttpClient(handler);

                // Gắn AccessToken nếu có trong cookie
                var token = Request.Cookies["AccessToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                // Gọi API backend: GET https://localhost:7182/api/Campaign/list?page={page}
                var apiUrl = $"{CAMPAIGN_LIST_API}?page={page}";
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var apiResult = JsonSerializer.Deserialize<CampaignApiResponse>(responseContent, options);

                    if (apiResult?.Data != null)
                    {
                        model.PagedData = apiResult.Data;
                        model.IsApiSuccess = true;
                    }
                    else
                    {
                        model.IsApiSuccess = false;
                        model.ErrorMessage = apiResult?.Message ?? "Không có dữ liệu trả về từ API.";
                    }
                }
            }
            catch (Exception ex)
            {
                model.IsApiSuccess = false;
                model.ErrorMessage = $"Không thể kết nối đến {CAMPAIGN_LIST_API}: {ex.Message}";
                model.PagedData = GetFallbackData(page);
                model.IsUsingFallback = true;
            }

            return View(model);
        }

      
        // POST: Tạo chiến dịch mới → gọi API https://localhost:7182/api/Campaign/create
        [HttpPost]
        [Route("Admin/Campaign/Create")]
        public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignRequest request)
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                using var client = new HttpClient(handler);

                string? token = null;
                if (Request.Headers.TryGetValue("Authorization", out var authHeader) && !string.IsNullOrEmpty(authHeader))
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
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(CAMPAIGN_CREATE_API, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return Json(new { success = true, message = "Chiến dịch đã được tạo thành công!" });

                return Json(new { success = false, message = $"API lỗi {(int)response.StatusCode}: {responseBody}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Không thể kết nối backend: {ex.Message}" });
            }
        }

        // GET: Tìm kiếm cán bộ y tế → gọi API GET https://localhost:7182/api/Staff/search?text={text}
        [HttpGet]
        [Route("Admin/Staff/Search")]
        public async Task<IActionResult> SearchStaff(string? text = "", string? query = "")
        {
            var searchText = !string.IsNullOrWhiteSpace(text) ? text : query;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return Json(new { message = "Enter a name or email to search for staff.", data = new List<object>() });
            }

            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                using var client = new HttpClient(handler);

                // Lấy AccessToken từ Header hoặc Cookie
                string? token = null;
                if (Request.Headers.TryGetValue("Authorization", out var authHeader) && !string.IsNullOrEmpty(authHeader))
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

                // Backend ServerMedical yêu cầu đúng tham số ?text=...
                var url = $"{STAFF_SEARCH_API}?text={Uri.EscapeDataString(searchText.Trim())}";

                var response = await client.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Content(responseBody, "application/json");
                }

                return StatusCode((int)response.StatusCode, responseBody);
            }
            catch (Exception ex)
            {
                return StatusCode(500, JsonSerializer.Serialize(new { message = $"Không thể kết nối backend: {ex.Message}", data = new List<object>() }));
            }
        }

        // DELETE: Xóa chiến dịch → gọi API DELETE https://localhost:7182/api/Campaign/delete/{id}
        [HttpDelete]
        [Route("Admin/Campaign/Delete/{id:int}")]
        public async Task<IActionResult> DeleteCampaign(int id)
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                using var client = new HttpClient(handler);

                // Lấy AccessToken từ Authorization Header hoặc Cookie
                string? token = null;
                if (Request.Headers.TryGetValue("Authorization", out var authHeader) && !string.IsNullOrEmpty(authHeader))
                {
                    var authStr = authHeader.ToString().Trim();
                    token = authStr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? authStr.Substring(7).Trim()
                        : authStr;
                }
                if (string.IsNullOrEmpty(token))
                    token = Request.Cookies["AccessToken"];

                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.DeleteAsync($"{CAMPAIGN_DELETE_API}/{id}");
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return Json(new { success = true, message = "Chiến dịch đã được xóa thành công!" });

                return Json(new { success = false, message = $"API lỗi {(int)response.StatusCode}: {responseBody}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Không thể kết nối backend: {ex.Message}" });
            }
        }

        // Menu 2: Quản lý User
        [HttpGet]
        [Route("Admin/Users")]
        public IActionResult Users()
        {
            return View();
        }

        /// <summary>
        /// Dữ liệu mẫu khi backend API chưa khởi động
        /// </summary>
        private CampaignPagedData GetFallbackData(int page)
        {
            var fallbackItems = new List<CampaignItem>
            {
                new CampaignItem
                {
                    CampaignId = 1,
                    Title = "Chiến dịch hiến máu tình nguyện - Hành Trình Đỏ 2026",
                    Location = "Học viện Công nghệ Bưu chính Viễn thông (PTIT)",
                    EventDate = DateTime.Parse("2026-12-20T08:00:00"),
                    AdminId = 2, StaffCount = 3, StaffAssignments = new List<object>()
                },
                new CampaignItem
                {
                    CampaignId = 2,
                    Title = "Giọt Máu Nghĩa Tình - Mùa Đông Ấm",
                    Location = "Nhà văn hóa Thanh niên, Quận 1, TP.HCM",
                    EventDate = DateTime.Now.AddDays(7),
                    AdminId = 2, StaffCount = 4, StaffAssignments = new List<object>()
                },
                new CampaignItem
                {
                    CampaignId = 3,
                    Title = "Blouse Trắng - Trái Tim Hồng 2026",
                    Location = "Bệnh viện Bạch Mai, Hà Nội",
                    EventDate = DateTime.Now.AddDays(-10),
                    AdminId = 1, StaffCount = 5, StaffAssignments = new List<object>()
                }
            };

            return new CampaignPagedData
            {
                Page = page, PageSize = 10,
                TotalItems = 25, TotalPages = 3,
                Items = fallbackItems
            };
        }
    }
}
