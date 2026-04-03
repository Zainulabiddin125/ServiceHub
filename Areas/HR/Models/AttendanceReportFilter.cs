namespace ServiceHub.Areas.HR.Models
{
    public class AttendanceReportFilter
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }

        // "Detail" | "Monthly" | "Absentee"
        public string ReportType { get; set; } = "Detail";
    }
}
