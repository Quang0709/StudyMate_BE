using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Model
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTime ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsUsed { get; set; }
    }
}
