﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceHub.Areas.HR.Models;
using ServiceHub.Data;

namespace ServiceHub.Areas.HR.Controllers
{
    [Area("HR")]
    [Authorize]
    public class AttendanceMachineController : Controller
    {
        private readonly ServiceHubContext _dbcontext;
        public AttendanceMachineController(ServiceHubContext context)
        {
            _dbcontext = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _dbcontext.AttendenceMachines.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> GetAttendanceMachines()
        {
            var request = HttpContext.Request.Form;
            // DataTables parameters
            var draw = request["draw"].FirstOrDefault();
            var start = request["start"].FirstOrDefault();
            var length = request["length"].FirstOrDefault();
            var searchValue = request["search[value]"].FirstOrDefault();
            var sortColumn = request["columns[" + request["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortDirection = request["order[0][dir]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // Query data from the database
            var query = _dbcontext.AttendenceMachines.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchValue))
            {
                var search = searchValue.ToLower();

                query = query.Where(m =>
                    m.Name.Contains(searchValue) ||
                    m.IpAddress.Contains(searchValue) ||
                    m.Description.Contains(searchValue) ||
                    m.Location.Contains(searchValue) ||
                    (search == "yes" && m.IsActive) ||
                    (search == "no" && !m.IsActive) ||
                    (search == "all" && m.IsFetchAll) ||
                    (search == "latest" && !m.IsFetchAll)
                );
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(sortColumn))
            {
                switch (sortColumn)
                {
                    case "name":
                        query = sortDirection == "asc" ? query.OrderBy(m => m.Name) : query.OrderByDescending(m => m.Name);
                        break;
                    case "ipAddress":
                        query = sortDirection == "asc" ? query.OrderBy(m => m.IpAddress) : query.OrderByDescending(m => m.IpAddress);
                        break;
                    case "port":
                        query = sortDirection == "asc" ? query.OrderBy(m => m.Port) : query.OrderByDescending(m => m.Port);
                        break;
                    case "description":
                        query = sortDirection == "asc" ? query.OrderBy(m => m.Description) : query.OrderByDescending(m => m.Description);
                        break;
                    case "location":
                        query = sortDirection == "asc" ? query.OrderBy(m => m.Location) : query.OrderByDescending(m => m.Location);
                        break;
                    case "isActive":
                        query = sortDirection == "asc" ? query.OrderBy(m => m.IsActive) : query.OrderByDescending(m => m.IsActive);
                        break;
                    case "isFetchAll":
                        query = sortDirection == "asc" ? query.OrderBy(m => m.IsFetchAll) : query.OrderByDescending(m => m.IsFetchAll);
                        break;
                }
            }

            // Get total records count
            int totalRecords = await query.CountAsync();


            // Paginate data
            var data = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    ipAddress = m.IpAddress,
                    port = m.Port,
                    description = m.Description,
                    location = m.Location,
                    isActive = m.IsActive,
                    isFetchAll = m.IsFetchAll
                })
                .ToListAsync();

            // Return JSON response
            return Json(new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = data
            });
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,IpAddress,Port,IsActive,IsFetchAll,Location,Description,DeviceModel,CreatedAt,LastUpdated")] AttendanceMachine attendanceMachine)
        {           

            if (ModelState.IsValid)
            {
                if (attendanceMachine.Port < 1 || attendanceMachine.Port > 65535)
                {
                    ModelState.AddModelError("Port", "Port must be between 1 and 65535.");
                    return View(attendanceMachine);
                }
                attendanceMachine.CreatedAt = DateTime.UtcNow;
                attendanceMachine.IsActive = attendanceMachine.IsActive; 
                attendanceMachine.IsFetchAll = attendanceMachine.IsFetchAll;
                _dbcontext.AttendenceMachines.Add(attendanceMachine);
                await _dbcontext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(attendanceMachine);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var attendanceMachine = await _dbcontext.AttendenceMachines.FindAsync(id);
            if (attendanceMachine == null)
            {
                return NotFound();
            }
            return View(attendanceMachine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IpAddress,Port,IsActive,IsFetchAll,Location,Description,DeviceModel,CreatedAt,LastUpdated")] AttendanceMachine attendanceMachine)
        {
            if (id != attendanceMachine.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (attendanceMachine.Port < 1 || attendanceMachine.Port > 65535)
                    {
                        ModelState.AddModelError("Port", "Port must be between 1 and 65535.");
                        return View(attendanceMachine);
                    }
                    attendanceMachine.IsActive = attendanceMachine.IsActive;
                    attendanceMachine.IsFetchAll = attendanceMachine.IsFetchAll;
                    attendanceMachine.LastUpdated = DateTime.UtcNow;
                    _dbcontext.Update(attendanceMachine);
                    await _dbcontext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AttendanceMachineExists(attendanceMachine.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(attendanceMachine);
        }

        private bool AttendanceMachineExists(int id)
        {
            return _dbcontext.AttendenceMachines.Any(e => e.Id == id);
        }      
    }
}
