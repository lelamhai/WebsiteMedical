using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebsiteMedical.Models;

namespace WebsiteMedical.Controllers
{
    public class AdminController : Controller
    {
        public const string BASE_URL = "https://localhost:7182";
        private readonly string CAMPAIGN_LIST_API = $"{BASE_URL}/api/Campaign/list";

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
               
            }

            return View(model);
        }

      
        // Menu 2: Quản lý User
        [HttpGet]
        [Route("Admin/Users")]
        public IActionResult Users()
        {
            return View();
        }
    }
}
