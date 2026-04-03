namespace ServiceHub.Areas.HR.Models
{
    public class MachineHealthSummary
    {
        public int TotalMachines { get; set; }
        public int OnlineMachines { get; set; }
        public int OfflineMachines { get; set; }
    }
}
