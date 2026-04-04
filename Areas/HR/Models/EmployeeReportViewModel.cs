namespace ServiceHub.Areas.HR.Models
{
    // ───────────────────────────────────────────────────────────────────────
    //  List / Search row
    // ───────────────────────────────────────────────────────────────────────
    public class EmployeeReportListRow
    {
        public string EmpNo { get; set; } = string.Empty;
        public string EmpName { get; set; } = string.Empty;

        // Biometric
        public int TotalFingersEnrolled { get; set; }       // sum across ALL machines
        public int MachinesAssigned { get; set; }
        public string BiometricStatus { get; set; } = "Not Enrolled"; // Active | Not Enrolled

        // System
        public DateTime? EnrollmentDate { get; set; }       // first EmployeeEnrollment.CreatedAt
        public DateTime? LastAttendance { get; set; }       // MAX(HR_Swap_Record.Swap_Time)
        public bool IsActive { get; set; } = true;

        // Fraud alert flag — more than 10 distinct finger indexes enrolled for this employee
        public bool FraudAlert { get; set; }
        public string? FraudReason { get; set; }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  Detail page — per-machine breakdown
    // ───────────────────────────────────────────────────────────────────────
    public class MachineEnrollmentDetail
    {
        public int MachineId { get; set; }
        public string MachineIP { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public int FingersEnrolled { get; set; }
        public string FingerIndexes { get; set; } = string.Empty;     // "0,1,2"
        public string FingerNames { get; set; } = string.Empty;       // "Right Thumb, Right Index, Right Middle"
        public DateTime? EnrollmentDate { get; set; }
        public bool IsActive { get; set; }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  Full detail page viewmodel
    // ───────────────────────────────────────────────────────────────────────
    public class EmployeeDetailViewModel
    {
        // ── Basic Information ────────────────────────────────────────────
        public string EmpNo { get; set; } = string.Empty;
        public string EmpName { get; set; } = string.Empty;
        public string Department { get; set; } = "N/A";
        public string Designation { get; set; } = "N/A";
        public string Branch { get; set; } = "N/A";

        // ── Biometric Information ────────────────────────────────────────
        public int TotalFingersEnrolled { get; set; }
        public int UniqueFingersAcrossAllMachines { get; set; }     // distinct finger indexes
        public string BiometricStatus { get; set; } = "Not Enrolled";
        public bool PasswordSet { get; set; }
        public List<MachineEnrollmentDetail> AssignedMachines { get; set; } = new();

        // ── System Information ───────────────────────────────────────────
        public DateTime? EnrollmentDate { get; set; }
        public DateTime? LastAttendance { get; set; }
        public string? LastAttendanceMachine { get; set; }
        public bool IsActive { get; set; } = true;
        public string? CreatedBy { get; set; }

        // ── Fraud Detection ──────────────────────────────────────────────
        /// <summary>True when suspicious fingerprint activity is detected.</summary>
        public bool FraudAlert { get; set; }
        public string? FraudReason { get; set; }

        // ── Recent Attendance (last 10 punches) ──────────────────────────
        public List<AttendancePunchRow> RecentAttendance { get; set; } = new();
    }

    public class AttendancePunchRow
    {
        public DateTime? SwapTime { get; set; }
        public string? MachineIP { get; set; }
        public string? MachineName { get; set; }
        public string Direction { get; set; } = string.Empty;  // "In" | "Out" | "–"
    }
}
