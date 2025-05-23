﻿namespace ServiceHub.Models
{
    public class TransferEmployeeRequest
    {
        public string SourceIP { get; set; } // IP address of the source machine
        public string DestinationIP { get; set; } // IP address of the destination machine
        public string EmpNo { get; set; } 
        public string EmpName { get; set; } 
        public string UserId { get; set; } 
    }
}
