namespace ServiceHub.Areas.HR.Models
{
    public class PasswordChangeLog
    {
        public int Id { get; set; }
        public string IP { get; set; }
        public string UserID { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
