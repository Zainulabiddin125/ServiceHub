using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
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
            var draw = request["draw"].FirstOrDefault();
            var start = request["start"].FirstOrDefault();
            var length = request["length"].FirstOrDefault();
            var searchValue = request["search[value]"].FirstOrDefault();
            var sortColumnIndex = request["order[0][column]"].FirstOrDefault();
            var sortColumnName = request[$"columns[{sortColumnIndex}][data]"].FirstOrDefault();
            var sortDirection = request["order[0][dir]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var query = _dbcontext.AttendenceMachineConnectionLogs.AsQueryable();

            // 🟡 Universal search filter on all relevant columns
            if (!string.IsNullOrEmpty(searchValue))
            {
                searchValue = searchValue.ToLower();

                query = query.Where(m =>
                    m.Id.ToString().Contains(searchValue) ||
                    m.MachineId.ToString().ToLower().Contains(searchValue) ||
                    m.Machine_IP.ToLower().Contains(searchValue) ||
                    m.Connection_StartTime.ToString("dd-MMM-yyyy hh:mm tt").ToLower().Contains(searchValue) ||
                    (m.Connection_EndTime.HasValue && m.Connection_EndTime.Value.ToString("dd-MMM-yyyy hh:mm tt").ToLower().Contains(searchValue)) ||
                    m.Status.ToLower().Contains(searchValue) ||
                    (!string.IsNullOrEmpty(m.ErrorMessage) && m.ErrorMessage.ToLower().Contains(searchValue)) ||
                    m.RecordsRead.ToString().Contains(searchValue)
                );
            }

            // 🔁 Dynamic sort logic
            if (!string.IsNullOrEmpty(sortColumnName) && !string.IsNullOrEmpty(sortDirection))
            {
                var isAsc = sortDirection == "asc";
                query = sortColumnName switch
                {
                    "id" => isAsc ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id),
                    "machineId" => isAsc ? query.OrderBy(x => x.MachineId) : query.OrderByDescending(x => x.MachineId),
                    "machine_IP" => isAsc ? query.OrderBy(x => x.Machine_IP) : query.OrderByDescending(x => x.Machine_IP),
                    "connection_StartTime" => isAsc ? query.OrderBy(x => x.Connection_StartTime) : query.OrderByDescending(x => x.Connection_StartTime),
                    "connection_EndTime" => isAsc ? query.OrderBy(x => x.Connection_EndTime) : query.OrderByDescending(x => x.Connection_EndTime),
                    "status" => isAsc ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status),
                    "errorMessage" => isAsc ? query.OrderBy(x => x.ErrorMessage) : query.OrderByDescending(x => x.ErrorMessage),
                    "recordsRead" => isAsc ? query.OrderBy(x => x.RecordsRead) : query.OrderByDescending(x => x.RecordsRead),
                    _ => query.OrderByDescending(x => x.Id)
                };
            }

            var totalRecords = await query.CountAsync();

            var queryData = await query.Skip(skip).Take(pageSize).ToListAsync();
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
        [HttpGet]
        public async Task<IActionResult> ExportConnectionLogs(string search = null, string sortColumn = null, string sortDirection = null)
        {
            var query = _dbcontext.AttendenceMachineConnectionLogs.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                var searchValue = search.ToLower();

                query = query.Where(m =>
                    m.Id.ToString().Contains(searchValue) ||
                    m.MachineId.ToString().Contains(searchValue) ||
                    m.Machine_IP.ToLower().Contains(searchValue) ||
                    m.Connection_StartTime.ToString("dd-MMM-yyyy hh:mm tt").Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                    (m.Connection_EndTime.HasValue && m.Connection_EndTime.Value.ToString("dd-MMM-yyyy hh:mm tt").Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                    m.Status.ToLower().Contains(searchValue) ||
                    (!string.IsNullOrEmpty(m.ErrorMessage) && m.ErrorMessage.ToLower().Contains(searchValue)) ||
                    m.RecordsRead.ToString().Contains(searchValue)
                );
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                var isAsc = sortDirection == "asc";
                query = sortColumn switch
                {
                    "id" => isAsc ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id),
                    "machineId" => isAsc ? query.OrderBy(x => x.MachineId) : query.OrderByDescending(x => x.MachineId),
                    "machine_IP" => isAsc ? query.OrderBy(x => x.Machine_IP) : query.OrderByDescending(x => x.Machine_IP),
                    "connection_StartTime" => isAsc ? query.OrderBy(x => x.Connection_StartTime) : query.OrderByDescending(x => x.Connection_StartTime),
                    "connection_EndTime" => isAsc ? query.OrderBy(x => x.Connection_EndTime) : query.OrderByDescending(x => x.Connection_EndTime),
                    "status" => isAsc ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status),
                    "errorMessage" => isAsc ? query.OrderBy(x => x.ErrorMessage) : query.OrderByDescending(x => x.ErrorMessage),
                    "recordsRead" => isAsc ? query.OrderBy(x => x.RecordsRead) : query.OrderByDescending(x => x.RecordsRead),
                    _ => query.OrderByDescending(x => x.Id)
                };
            }

            // Fetch all filtered & sorted records
            var records = await query.Select(m => new
            {
                Id = m.Id,
                MachineId = m.MachineId,
                Machine_IP = m.Machine_IP,
                Connection_StartTime = m.Connection_StartTime.ToString("dd-MMM-yyyy hh:mm tt"),
                Connection_EndTime = m.Connection_EndTime.HasValue
                                     ? m.Connection_EndTime.Value.ToString("dd-MMM-yyyy hh:mm tt")
                                     : "N/A",
                Status = m.Status,
                ErrorMessage = m.ErrorMessage ?? "N/A",
                RecordsRead = m.RecordsRead,
                LastFetching = GetConnectionTimeAgo(m.Connection_StartTime, DateTime.Now)
            }).ToListAsync();

            // Optional: Limit max number of records to prevent memory issues
            //const int MAX_RECORDS = 100_000;
            //if (records.Count > MAX_RECORDS)
            //{
            //    return BadRequest($"Too many records ({records.Count}). Please refine your filters.");
            //}

            // Generate Excel file
            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet = workbook.Worksheets.Add("Connection Logs");

                // Headers
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Machine ID";
                worksheet.Cell(1, 3).Value = "Machine IP";
                worksheet.Cell(1, 4).Value = "Connection Start Time";
                worksheet.Cell(1, 5).Value = "Connection End Time";
                worksheet.Cell(1, 6).Value = "Status";
                worksheet.Cell(1, 7).Value = "Error Message";
                worksheet.Cell(1, 8).Value = "Records Read";
                worksheet.Cell(1, 9).Value = "Last Fetching";

                int row = 2;
                foreach (var record in records)
                {
                    worksheet.Cell(row, 1).Value = record.Id;
                    worksheet.Cell(row, 2).Value = record.MachineId;
                    worksheet.Cell(row, 3).Value = record.Machine_IP;
                    worksheet.Cell(row, 4).Value = record.Connection_StartTime;
                    worksheet.Cell(row, 5).Value = record.Connection_EndTime;
                    worksheet.Cell(row, 6).Value = record.Status;
                    worksheet.Cell(row, 7).Value = record.ErrorMessage;
                    worksheet.Cell(row, 8).Value = record.RecordsRead;
                    worksheet.Cell(row, 9).Value = record.LastFetching;

                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MachineConnectionLogs.xlsx");
                }
            }
        }
        private static string GetConnectionTimeAgo(DateTime startTime, DateTime currentTime)
        {
            var timeSpan = currentTime - startTime;
            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} hours ago";
            return $"{(int)timeSpan.TotalDays} days ago";
        }
    }
}

