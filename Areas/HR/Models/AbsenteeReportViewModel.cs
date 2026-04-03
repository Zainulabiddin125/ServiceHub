namespace ServiceHub.Areas.HR.Models
{
    public class AbsenteeReportViewModel
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public DateTime Date { get; set; }
        public string DateLabel => Date.ToString("dd-MMM-yyyy");
        public string DayName => Date.ToString("dddd");
    }
}
