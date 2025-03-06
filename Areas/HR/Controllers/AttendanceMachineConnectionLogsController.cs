using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceHub.Areas.HR.Models;
using ServiceHub.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceHub.Areas.HR.Controllers
{
    [Area("HR")]
    public class AttendanceMachineConnectionLogsController : Controller
    {
        private readonly ServiceHubContext _dbcontext;

        public AttendanceMachineConnectionLogsController(ServiceHubContext context)
        {
            _dbcontext = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetConnectionLogs()
        {
            var request = HttpContext.Request.Form;
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var sortColumn = request["columns[" + request["order[0][column]"].FirstOrDefault() + "][id]"].FirstOrDefault();
            var sortDirection = request["order[0][dir]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            var query = _dbcontext.AttendenceMachineConnectionLogs.AsQueryable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(m => m.Machine_IP.Contains(searchValue) ||
                                         m.Status.Contains(searchValue));
            }
            // Apply sorting
            if (!string.IsNullOrEmpty(sortColumn))
            {
                switch (sortColumn)
                {
                    case "id":
                        query = sortDirection == "asc" ? query.OrderBy(m => m.Id) : query.OrderByDescending(m => m.Id);
                        break;
                    case "machineId":
                        query = sortDirection == "asc" ? query.OrderBy(m => m.MachineId) : query.OrderByDescending(m => m.MachineId);
                        break;
                    case "machine_IP":
                        query = sortDirection == "asc" ? query.OrderBy(m => m.Machine_IP) : query.OrderByDescending(m => m.Machine_IP);
                        break;
                    case "status":
                        query = sortDirection == "asc" ? query.OrderBy(m => m.Status) : query.OrderByDescending(m => m.Status);
                        break;
                }
            }
            // Get total records count
            int totalRecords = await query.CountAsync();

            var queryData = await query
                                .Skip(skip)
                                .Take(pageSize)
                                .ToListAsync();
            DateTime currentTime = DateTime.Now; 

            var data = queryData.Select(m => new
            {
                id = m.Id,
                machineId = m.MachineId,
                machine_IP = m.Machine_IP,
                connection_StartTime = m.Connection_StartTime.ToString("dd-MMM-yyyy hh:mm tt"),
                connection_EndTime = m.Connection_EndTime.HasValue ? m.Connection_EndTime.Value.ToString("dd-MMM-yyyy hh:mm tt") : "N/A",
                status = m.Status,
                errorMessage = m.ErrorMessage,
                recordsRead = m.RecordsRead,
                lastFetching = GetTimeAgo(m.Connection_StartTime, currentTime)
            }).ToList();

            // Return JSON response
            return Json(new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = data
            });
        }
        private string GetTimeAgo(DateTime startTime, DateTime currentTime)
        {
            var timeSpan = currentTime - startTime;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";

            return $"{(int)timeSpan.TotalDays} days ago";
        }
    }
}

