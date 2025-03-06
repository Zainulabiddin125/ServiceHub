using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceHub.Areas.HR.Models
{
    public class HRSwapRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PK_line_id { get; set; }
        [StringLength(10)]
        public string Emp_No { get; set; }
        public DateTime? Swap_Time { get; set; }
        public bool Manual { get; set; }
        public bool? Shift_In { get; set; } = false;
        public bool? Shift_Out { get; set; } = false;
        public DateTime? Creation_Date { get; set; } = DateTime.Now;
        [StringLength(20)]
        public string? Machine_IP { get; set; }
        [StringLength(20)]
        public string? Machine_Port { get; set; }
        public int? MachineId { get; set; }
    }
}
