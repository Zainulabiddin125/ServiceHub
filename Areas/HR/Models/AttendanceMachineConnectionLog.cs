using System.ComponentModel.DataAnnotations;
namespace ServiceHub.Areas.HR.Models
{
    public class AttendanceMachineConnectionLog
    {
        public int Id { get; set; }
        public int MachineId { get; set; }
        public string Machine_IP { get; set; }
        public DateTime Connection_StartTime { get; set; }
        public DateTime? Connection_EndTime { get; set; }
        public string Status { get; set; }
        public string? ErrorMessage { get; set; }
        public int? RecordsRead { get; set; }
    }
}
