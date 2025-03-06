using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceHub.Data;
using System.Reflection.PortableExecutable;

namespace ServiceHub.Areas.HR.Controllers
{
    [Area("HR")]
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

        [HttpPost]
        public async Task<IActionResult> GetAttendanceRecords()
        {
            var request = HttpContext.Request.Form;
            // DataTables parameters
            var draw = request["draw"].FirstOrDefault();
            var start = request["start"].FirstOrDefault();
            var length = request["length"].FirstOrDefault();
            var searchValue = request["search[value]"].FirstOrDefault();
            var sortColumnIndex = request["order[0][column]"].FirstOrDefault();
            var sortDirection = request["order[0][dir]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var query = _dbcontext.HR_Swap_Record
                .OrderByDescending(m => m.PK_line_id)
                .Take(5000)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(m =>
                    m.Emp_No.Contains(searchValue) ||
                    (m.Swap_Time.HasValue && m.Swap_Time.Value.ToString().Contains(searchValue)) ||
                    (m.Creation_Date.HasValue && m.Creation_Date.Value.ToString().Contains(searchValue)) ||
                    m.Machine_IP.Contains(searchValue)
                );
            }

            // Sorting
            if (!string.IsNullOrEmpty(sortColumnIndex))
            {
                int columnIndex = int.Parse(sortColumnIndex);
                bool isAscending = sortDirection == "asc";

                query = columnIndex switch
                {
                    0 => isAscending ? query.OrderBy(m => m.PK_line_id) : query.OrderByDescending(m => m.PK_line_id),
                    1 => isAscending ? query.OrderBy(m => m.Emp_No) : query.OrderByDescending(m => m.Emp_No),
                    2 => isAscending ? query.OrderBy(m => m.Swap_Time) : query.OrderByDescending(m => m.Swap_Time),
                    3 => isAscending ? query.OrderBy(m => m.Creation_Date) : query.OrderByDescending(m => m.Creation_Date),
                    4 => isAscending ? query.OrderBy(m => m.Machine_IP) : query.OrderByDescending(m => m.Machine_IP),
                    _ => query
                };
            }

            // Total records count
            int totalRecords = await query.CountAsync();

            // Apply pagination
            var queryData = await query
                                .Skip(skip)
                                .Take(pageSize)
                                .ToListAsync();

            // Transform data
            var data = queryData.Select(m => new
            {
                id = m.PK_line_id,
                emp_No = m.Emp_No,
                swap_Time = m.Swap_Time.HasValue ? m.Swap_Time.Value.ToString("dd-MMM-yyyy hh:mm tt") : "",
                creation_Date = m.Creation_Date.HasValue ? m.Creation_Date.Value.ToString("dd-MMM-yyyy hh:mm tt") : "",
                machine_IP = m.Machine_IP,
                shift_In = m.Shift_In,
                shift_Out = m.Shift_Out,
                machine_Port = m.Machine_Port,
                machineId = m.MachineId,
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



    }
}
