using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace FileSharingSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
