namespace ServiceHub.Areas.HR.Models
{
    public class AttendanceReportViewModel
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public int? LocationId { get; set; }

        // Date only — displayed as dd-MMM-yyyy
        public DateTime Date { get; set; }
        public string DateLabel => Date.ToString("dd-MMM-yyyy");

        // Min punch of the day
        public DateTime? CheckIn { get; set; }
        public string CheckInLabel => CheckIn.HasValue
            ? CheckIn.Value.ToString("hh:mm tt") : "Absent";

        // Max punch of the day
        public DateTime? CheckOut { get; set; }
        public string CheckOutLabel => CheckOut.HasValue
            ? CheckOut.Value.ToString("hh:mm tt") : "Absent";

        // CheckOut - CheckIn in hours (null = absent or only one punch)
        public double? TotalWorkingHours { get; set; }
        public string TotalHoursLabel { get; set; }   // e.g. "07h 45m"

        // Standard workday in hours — 12 hours per requirement
        public const double StandardHours = 12.0;

        // Late = worked but less than 12 h  (absent rows are flagged separately)
        public bool IsLate { get; set; }

        // > 12 h: how many extra hours
        public double? OvertimeHours { get; set; }
        public string OvertimeLabel { get; set; }   // e.g. "01h 20m" or "—"

        // True when no punch records exist for this employee on this date
        public bool IsAbsent { get; set; }
    }
}
