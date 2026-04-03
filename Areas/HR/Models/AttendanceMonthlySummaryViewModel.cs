namespace ServiceHub.Areas.HR.Models
{
    public class AttendanceMonthlySummaryViewModel
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthLabel => new DateTime(Year, Month, 1).ToString("MMMM yyyy");

        public int TotalDays { get; set; }   // working days in the month
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LateDays { get; set; }
        public double TotalWorkedHours { get; set; }
        public string TotalWorkedLabel { get; set; }
        public double TotalOvertimeHours { get; set; }
        public string TotalOvertimeLabel { get; set; }
    }
}
