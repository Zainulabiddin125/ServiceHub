namespace ServiceHub.Areas.HR.Models
{
    public class DashboardViewModel
    {
        public int SuccessfullyConnected { get; set; }
        public int FailedConnections { get; set; }
        public int TotalRecordsFetched { get; set; }
        public int ActiveMachines { get; set; }
        public int InactiveMachines { get; set; }
        public List<string> ChartLabels { get; set; }
        public List<int> ChartData { get; set; }
    }
}
