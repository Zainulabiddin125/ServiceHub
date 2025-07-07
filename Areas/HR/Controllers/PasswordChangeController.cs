using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using ServiceHub.Areas.HR.Models;
using ServiceHub.Controllers;
using ServiceHub.Data;
using System.Globalization;

namespace ServiceHub.Areas.HR.Controllers
{
    [Area("HR")]
    [Authorize]
    public class PasswordChangeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ServiceHubContext _context;
        private readonly TimeWindowService _timeWindowService;

        public PasswordChangeController(IConfiguration configuration, ServiceHubContext context, TimeWindowService timeWindowService)
        {
            _configuration = configuration;
            _context = context;
            _timeWindowService = timeWindowService;
        }

        public IActionResult Index()
        {
            ViewBag.TransferWindowMessage = _timeWindowService.GetTransferWindowMessage();
            ViewBag.IsTransferWindowOpen = _timeWindowService.IsTransferWindowOpen();
            ViewBag.NextWindowChange = _timeWindowService.GetNextWindowChange()?.TotalMilliseconds;
            return View();
        }

        

        [HttpGet]
        public async Task<IActionResult> GetMachinePort(string ip)
        {
            var port = await _context.AttendenceMachines
                .Where(m => m.IpAddress == ip)
                .Select(m => m.Port)
                .FirstOrDefaultAsync();
            return Json(port);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeRequest request)
        {
            if (!_timeWindowService.IsTransferWindowOpen())
            {
                return StatusCode(403, new { success = false, message = _timeWindowService.GetTransferWindowMessage() });
            }

            using (var client = new HttpClient())
            {
                var response = await client.PostAsJsonAsync("http://localhost:5000/api/updatepassword/", request);
                var content = await response.Content.ReadAsStringAsync();
                // Return response if not even HTTP success
                if (!response.IsSuccessStatusCode)
                {
                    return Content(content, "application/json");
                }
                // Parse response content to extract success flag
                try
                {
                    var result = Newtonsoft.Json.Linq.JObject.Parse(content);
                    bool isSuccess = result["success"]?.Value<bool>() ?? false;

                    if (isSuccess)
                    {
                        var log = new PasswordChangeLog
                        {
                            IP = request.IP,
                            UserID = request.UserID,
                            OldPassword = PasswordEncryptionHelper.Encrypt(request.OldPassword),
                            NewPassword = PasswordEncryptionHelper.Encrypt(request.NewPassword),
                            ChangedAt = DateTime.Now
                        };
                        _context.PasswordChangeLog.Add(log);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    // log parsing failure if needed
                    return StatusCode(500, new { success = false, message = "Failed to parse response from password API. " + ex.Message });
                }

                return Content(content, "application/json");
            }
        }
    }
}
