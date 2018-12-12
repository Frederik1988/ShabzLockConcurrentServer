using System;
using System.Collections.Generic;
using System.Text;

namespace ShabzLockConcurrent.Model
{
    public class Log
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Date { get; set; }
        public bool Status { get; set; }
        public int LockId { get; set; }
        public Log(int accountId, string date, bool status, int lockId)
        {
            AccountId = accountId;
            Date = date;
            Status = status;
            LockId = lockId;
        }
    }
}
