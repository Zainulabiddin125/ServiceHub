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
        
    }
}
