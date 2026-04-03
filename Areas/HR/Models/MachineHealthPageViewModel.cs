namespace ServiceHub.Areas.HR.Models
{
    public class MachineHealthPageViewModel
    {
        public MachineHealthSummary Summary { get; set; }
        public List<MachineHealthViewModel> Machines { get; set; }
    }
}
