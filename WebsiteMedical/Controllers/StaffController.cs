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
        public IActionResult MedicalRecords()
        {
            return View();
        }
    }
}

