using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceHub.Areas.HR.Models
{
    public class MachineFormatLog
    {
        [Key]
        public int Id { get; set; }

        public int MachineId { get; set; }

        [StringLength(50)]
        public string MachineIP { get; set; }

        [StringLength(200)]
        public string? MachineName { get; set; }

        /// <summary>Success | Failed</summary>
        [StringLength(50)]
        public string Status { get; set; } = "Success";

        public string? ErrorMessage { get; set; }

        [StringLength(450)]
        public string? RequestedByUserId { get; set; }

        [StringLength(256)]
        public string? RequestedByUserName { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.Now;

        public DateTime? ExecutedAt { get; set; }

        [ForeignKey(nameof(MachineId))]
        public virtual AttendanceMachine? Machine { get; set; }
    }
}
