using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceHub.Data;
using System.Reflection.PortableExecutable;

namespace ServiceHub.Areas.HR.Controllers
{
    [Area("HR")]
    [Authorize]
    public class HRSwapRecordsController : Controller
    {
        private readonly ServiceHubContext _dbcontext;
        public HRSwapRecordsController(ServiceHubContext context)
        {
            _dbcontext = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetMachineIPs()
        {
            var machineIPs = await _dbcontext.AttendenceMachines
                .Where(m => m.IsActive == true)
                .Select(m => m.IpAddress)
                .Distinct()
                .ToListAsync();
            return Json(machineIPs);
        }

        [HttpPost]
        public async Task<IActionResult> GetAttendanceRecords(string ipAddress = null, string startDate = null, string endDate = null)
        {
            var request = HttpContext.Request.Form;
            var draw = request["draw"].FirstOrDefault();
            var start = request["start"].FirstOrDefault();
            var length = request["length"].FirstOrDefault();
            var searchValue = request["search[value]"].FirstOrDefault();
            var sortColumnIndex = request["order[0][column]"].FirstOrDefault();
            var sortDirection = request["order[0][dir]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var query = _dbcontext.HR_Swap_Record.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(ipAddress))
                query = query.Where(m => m.Machine_IP == ipAddress);

            if (DateTime.TryParse(startDate, out var startDt))
                query = query.Where(m => m.Swap_Time >= startDt);

            if (DateTime.TryParse(endDate, out var endDt))
                query = query.Where(m => m.Swap_Time <= endDt);

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(m =>
                    m.Emp_No.Contains(searchValue) ||
                    m.Emp_Name.Contains(searchValue) ||
                    (m.Swap_Time.HasValue && m.Swap_Time.Value.ToString().Contains(searchValue)) ||
                    (m.Creation_Date.HasValue && m.Creation_Date.Value.ToString().Contains(searchValue)) ||
                    m.Machine_IP.Contains(searchValue) ||
                    m.MachineId.ToString().Contains(searchValue));
            }

            // Sorting
            if (!string.IsNullOrEmpty(sortColumnIndex))
            {
                int colIndex = int.Parse(sortColumnIndex);
                bool asc = sortDirection == "asc";
                query = colIndex switch
                {
                    0 => asc ? query.OrderBy(m => m.PK_line_id) : query.OrderByDescending(m => m.PK_line_id),
                    1 => asc ? query.OrderBy(m => m.Emp_No) : query.OrderByDescending(m => m.Emp_No),
                    2 => asc ? query.OrderBy(m => m.Emp_Name) : query.OrderByDescending(m => m.Emp_Name),
                    3 => asc ? query.OrderBy(m => m.Swap_Time) : query.OrderByDescending(m => m.Swap_Time),
                    4 => asc ? query.OrderBy(m => m.Shift_In) : query.OrderByDescending(m => m.Shift_In),
                    5 => asc ? query.OrderBy(m => m.Shift_Out) : query.OrderByDescending(m => m.Shift_Out),
                    6 => asc ? query.OrderBy(m => m.Creation_Date) : query.OrderByDescending(m => m.Creation_Date),
                    7 => asc ? query.OrderBy(m => m.Machine_IP) : query.OrderByDescending(m => m.Machine_IP),
                    8 => asc ? query.OrderBy(m => m.Machine_Port) : query.OrderByDescending(m => m.Machine_Port),
                    9 => asc ? query.OrderBy(m => m.MachineId) : query.OrderByDescending(m => m.MachineId),
                    _ => query.OrderByDescending(m => m.PK_line_id),
                };
            }

            int totalRecords = await query.CountAsync();
            var dataList = await query.Skip(skip).Take(pageSize).ToListAsync();

            var data = dataList.Select(m => new
            {
                id = m.PK_line_id,
                emp_No = m.Emp_No,
                emp_Name = m.Emp_Name,
                swap_Time = m.Swap_Time?.ToString("dd-MMM-yyyy hh:mm tt") ?? "",
                shift_In = m.Shift_In,
                shift_Out = m.Shift_Out,
                creation_Date = m.Creation_Date?.ToString("dd-MMM-yyyy hh:mm tt") ?? "",
                machine_IP = m.Machine_IP,
                machine_Port = m.Machine_Port,
                machineId = m.MachineId,
            });

            return Json(new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = data
            });
        }
        [HttpGet]
        public async Task<IActionResult> ExportAttendanceRecords(string ipAddress = null, string startDate = null, string endDate = null, CancellationToken cancellationToken = default)
        {
            var query = _dbcontext.HR_Swap_Record.AsQueryable();

            if (!string.IsNullOrEmpty(ipAddress))
                query = query.Where(m => m.Machine_IP == ipAddress);

            if (DateTime.TryParse(startDate, out var startDt))
                query = query.Where(m => m.Swap_Time >= startDt);

            if (DateTime.TryParse(endDate, out var endDt))
                query = query.Where(m => m.Swap_Time <= endDt);

            // Only select necessary data to reduce memory usage
            var records = await query.Select(m => new
            {
                m.PK_line_id,
                m.Emp_No,
                m.Emp_Name,
                Swap_Time = m.Swap_Time.HasValue ? m.Swap_Time.Value.ToString("dd-MMM-yyyy hh:mm tt") : "",
                Shift_In = Convert.ToBoolean(m.Shift_In) ? "Yes" : "No",
                Shift_Out = Convert.ToBoolean(m.Shift_Out) ? "Yes" : "No",
                Creation_Date = m.Creation_Date.HasValue ? m.Creation_Date.Value.ToString("dd-MMM-yyyy hh:mm tt") : "",
                m.Machine_IP,
                m.Machine_Port,
                m.MachineId
            }).ToListAsync(cancellationToken);

            // Optional: Limit max number of records for safety
            const int MAX_EXPORT_RECORDS = 100_000;
            if (records.Count > MAX_EXPORT_RECORDS)
            {
                return BadRequest($"Too many records ({records.Count}). Please refine your filters to export fewer than {MAX_EXPORT_RECORDS} records.");
            }

            // Generate Excel
            using var workbook = new XLWorkbook();
            IXLWorksheet worksheet = workbook.Worksheets.Add("Attendance Swap Records");

            // Headers
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Emp #";
            worksheet.Cell(1, 3).Value = "Emp Name";
            worksheet.Cell(1, 4).Value = "Swap Time";
            worksheet.Cell(1, 5).Value = "Shift In";
            worksheet.Cell(1, 6).Value = "Shift Out";
            worksheet.Cell(1, 7).Value = "Creation Date";
            worksheet.Cell(1, 8).Value = "Machine IP";
            worksheet.Cell(1, 9).Value = "Machine Port";
            worksheet.Cell(1, 10).Value = "Machine ID";

            int row = 2;
            foreach (var record in records)
            {
                worksheet.Cell(row, 1).Value = record.PK_line_id;
                worksheet.Cell(row, 2).Value = record.Emp_No;
                worksheet.Cell(row, 3).Value = record.Emp_Name;
                worksheet.Cell(row, 4).Value = record.Swap_Time;
                worksheet.Cell(row, 5).Value = record.Shift_In;
                worksheet.Cell(row, 6).Value = record.Shift_Out;
                worksheet.Cell(row, 7).Value = record.Creation_Date;
                worksheet.Cell(row, 8).Value = record.Machine_IP;
                worksheet.Cell(row, 9).Value = record.Machine_Port;
                worksheet.Cell(row, 10).Value = record.MachineId;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AttendanceSwapRecords.xlsx");
        }
    }
}
