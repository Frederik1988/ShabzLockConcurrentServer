using System;
using System.Collections.Generic;
using System.Text;

namespace ShabzLockConcurrent.Model
{
    public class Account
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public int PrimaryLockId { get; set; }
        
        public Account(string email, string name, int primaryLockId)
        {
            Email = email;
            Name = name;
            PrimaryLockId = primaryLockId;
        }
        
    }
}
