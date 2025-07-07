namespace ServiceHub.Areas.HR.Models
{
    public class PasswordChangeRequest
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public string UserID { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
