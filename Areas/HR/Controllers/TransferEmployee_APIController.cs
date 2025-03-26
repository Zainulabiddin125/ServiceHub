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
            var machineIPs = await _dbcontext.AttendenceMachines.Where(m => m.IsActive == true)
                .Select(m => m.IpAddress)
                .Distinct()
                .ToListAsync();
            return Json(machineIPs);
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
        // Endpoint to transfer an employee

        [HttpPost]
        public async Task<IActionResult> TransferEmployee([FromBody] TransferEmployeeRequest transferRequest)
        {
            try
            {
                // Retrieve the current logged-in user's ID using ASP.NET Core Identity
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User is not authenticated.");
                }

                // Validate required fields
                if (string.IsNullOrEmpty(transferRequest.SourceIP) || string.IsNullOrEmpty(transferRequest.DestinationIP))
                {
                    return BadRequest("SourceIP and DestinationIP are required.");
                }

                if (string.IsNullOrEmpty(transferRequest.EmpNo) || string.IsNullOrEmpty(transferRequest.EmpName))
                {
                    return BadRequest("Employee data is invalid or missing.");
                }
                // Add the UserId to the transferRequest object
                transferRequest.UserId = userId;
                //Console.WriteLine($"Transferring employee {transferRequest.EmpNo} - {transferRequest.EmpName} from {transferRequest.SourceIP} to {transferRequest.DestinationIP}");

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(5);

                    // Forward the request to the external API
                    var response = await client.PostAsJsonAsync("http://localhost:5000/api/transfer/", transferRequest);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine($"Response from Windows Service: {responseContent}");

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        return Ok(result);
                    }
                    else
                    {
                        //Console.WriteLine($"Error from Windows Service: {responseContent}");
                        return StatusCode((int)response.StatusCode, $"Failed to transfer employee. Error: {responseContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }        
    }
}
