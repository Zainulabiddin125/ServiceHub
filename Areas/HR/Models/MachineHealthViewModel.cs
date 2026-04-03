namespace ServiceHub.Areas.HR.Models
{
    public class MachineHealthViewModel
    {
        public int MachineId { get; set; }
        public string MachineName { get; set; }
        public string MachineIP { get; set; }
        public int Port { get; set; }
        public string LocationName { get; set; }   // Branch / Location
        public int? LocationId { get; set; }

        // Status derived from last log record
        public string Status { get; set; }   // "Online" | "Offline"
        public bool IsOnline => Status == "Online";

        // Last time a successful sync completed
        public DateTime? LastCommunication { get; set; }

        // Human-readable "X minutes ago" label
        public string LastCommunicationLabel { get; set; }

        // Last count of records fetched
        public int? LastRecordsRead { get; set; }

        // Most recent error (if last status was Failed)
        public string LastErrorMessage { get; set; }
    }
}
