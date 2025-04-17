using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ServiceHub.Data;
using ServiceHub.Models;
using Newtonsoft.Json.Linq;

namespace ServiceHub.Areas.HR.Controllers
{

    [Area("HR")]
    [Authorize]
    public class TransferEmployee_APIController : Controller
    {
        private readonly ServiceHubContext _dbcontext;

        public TransferEmployee_APIController(ServiceHubContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public IActionResult Index()
        {
            return View();
        }
        // Endpoint to get machine IPs for the dropdown
        public async Task<IActionResult> GetMachineIPs()
        {
            var machineIPs = await _dbcontext.AttendenceMachines.Where(m => m.IsActive == true).Select(m => m.IpAddress).Distinct().ToListAsync();
            return Json(machineIPs);
        }

        // Endpoint to transfer employees

        [HttpPost]
        public async Task<IActionResult> TransferMultipleEmployees([FromBody] MultipleTransferRequest transferRequest)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User is not authenticated.");
                }
                // Call the service
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                    var response = await client.PostAsJsonAsync("http://localhost:5000/api/transfer/", new
                    {
                        transferRequest.SourceIP,
                        transferRequest.DestinationIPs,
                        transferRequest.Employees,
                        transferRequest.TransferAllEmployees,
                        transferRequest.TransferAllMachines,
                        UserId = userId
                    });
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        //return Ok(JsonConvert.DeserializeObject<dynamic>(responseContent));
                        return Content(responseContent, "application/json");
                    }
                    return StatusCode((int)response.StatusCode, responseContent);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Endpoint to fetch employees for specific IPs
        [HttpPost]
        public async Task<IActionResult> GetEmployees([FromBody] List<string> machineIPs)
        {
            using (var client = new HttpClient())
            {
                // Increase timeout to 5 minutes
                client.Timeout = TimeSpan.FromMinutes(5);
                try
                {
                    //Console.WriteLine($"Fetching employees for IPs: {string.Join(", ", machineIPs)}");
                    // Call the external API
                    var response = await client.PostAsJsonAsync("http://localhost:5000/api/employees/", machineIPs);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine($"Response from Windows Service: {responseContent}");

                    if (response.IsSuccessStatusCode)
                    {
                        // Parse response using JObject
                        var jsonObject = JObject.Parse(responseContent);
                        // Ensure "employees" exists and is an array
                        if (jsonObject["employees"] is JArray employeesArray)
                        {
                            var employees = new List<object>();
                            // Loop through the "employees" array
                            foreach (var item in employeesArray)
                            {
                                // Check if item is another array (nested structure)
                                if (item is JArray nestedArray)
                                {
                                    employees.AddRange(nestedArray.Select(emp => new
                                    {
                                        EmpNo = (string)emp["EmpNo"],
                                        EmpName = (string)emp["EmpName"]
                                    }));
                                }
                                else
                                {
                                    // Handle flat structure
                                    employees.Add(new
                                    {
                                        EmpNo = (string)item["EmpNo"],
                                        EmpName = (string)item["EmpName"]
                                    });
                                }
                            }
                            return Ok(employees);
                        }
                        return BadRequest("Invalid response format: 'employees' node missing or incorrect.");
                    }
                    else
                    {
                        //Console.WriteLine($"Error from Windows Service: {responseContent}");
                        return StatusCode((int)response.StatusCode, $"Failed to fetch employees. Error: {responseContent}");
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"Exception: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }
        }       

        // Model for multiple transfer request
        public class MultipleTransferRequest
        {
            public string SourceIP { get; set; }
            public List<string> DestinationIPs { get; set; }
            public List<EmployeeTransfer> Employees { get; set; }
            public bool TransferAllEmployees { get; set; }
            public bool TransferAllMachines { get; set; }
        }

        public class EmployeeTransfer
        {
            public string EmpNo { get; set; }
            public string EmpName { get; set; }
        }
    }
}