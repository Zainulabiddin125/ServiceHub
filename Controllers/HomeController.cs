using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceHub.Areas.HR.Models;
using ServiceHub.Data;
using ServiceHub.Models;
using System.Diagnostics;

namespace ServiceHub.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ServiceHubContext _dbcontext;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger,UserManager<ApplicationUser> userManager, ServiceHubContext context)
        {
            _logger = logger;
            this._userManager = userManager;
            _dbcontext = context;
        }

        public IActionResult Index()
        {
            ViewData["User_Id"]=_userManager.GetUserId(this.User);

            var currentDate = DateTime.Now.Date;

            // 1. Successfully Connected Devices
            var successfullyConnected = _dbcontext.AttendenceMachineConnectionLogs
                .Count(log => log.Connection_StartTime.Date == currentDate && log.Status == "Success");

            // 2. Failed Device Connections
            var failedConnections = _dbcontext.AttendenceMachineConnectionLogs
                .Count(log => log.Connection_StartTime.Date == currentDate && log.Status == "Failed");

            // 3. Total Records Fetched (Handling nullable Creation_Date)
            var totalRecordsFetched = _dbcontext.HR_Swap_Record
                .Count(record => record.Creation_Date.HasValue && record.Creation_Date.Value.Date == currentDate);

            // 4. Machines Status
            var activeMachines = _dbcontext.AttendenceMachines.Count(m => m.IsActive);
            var inactiveMachines = _dbcontext.AttendenceMachines.Count(m => !m.IsActive);

            // 5. Data for ApexCharts (Day-wise total records fetched for the current month)
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);

            var startDate = new DateTime(currentYear, currentMonth, 1); // First day of the current month
            var endDate = new DateTime(currentYear, currentMonth, daysInMonth); // Last day of the current month

            var recordsByDate = _dbcontext.HR_Swap_Record
                .Where(record => record.Creation_Date.HasValue &&
                                 record.Creation_Date.Value >= startDate &&
                                 record.Creation_Date.Value <= endDate)
                .GroupBy(record => record.Creation_Date.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToList();

            // Create a dictionary for quick lookup
            var recordsLookup = recordsByDate.ToDictionary(r => r.Date, r => r.Count);

            // Generate data for all days in the current month
            var chartLabels = new List<string>();
            var chartData = new List<int>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                chartLabels.Add(date.ToString("yyyy-MM-dd"));
                chartData.Add(recordsLookup.ContainsKey(date) ? recordsLookup[date] : 0);
            }

            var viewModel = new DashboardViewModel
            {
                SuccessfullyConnected = successfullyConnected,
                FailedConnections = failedConnections,
                TotalRecordsFetched = totalRecordsFetched,
                ActiveMachines = activeMachines,
                InactiveMachines = inactiveMachines,
                ChartLabels = chartLabels,
                ChartData = chartData
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
