using System;
using System.Collections.Generic;
using System.Text;

namespace ShabzLockConcurrent.Model
{
    public class Lock
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AccessCode { get; set; }
        public bool Status { get; set; }
        public string DateRegistered { get; set; }
        
        public Lock(string name, string accessCode, bool status, string dateRegistered)
        {
            Name = name;
            AccessCode = accessCode;
            Status = status;
            DateRegistered = dateRegistered;
        }
    }
}
