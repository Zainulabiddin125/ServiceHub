namespace ServiceHub.Areas.HR.Models
{
    public class AttendanceReportPageViewModel
    {
        public List<string> Departments { get; set; } = new();
        public List<LocationDropdown> Locations { get; set; } = new();
    }
    public class LocationDropdown
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
