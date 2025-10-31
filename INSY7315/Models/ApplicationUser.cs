using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace INSY7315.Models
{
    public class ApplicationUser : IdentityUser
    {
       
        [StringLength(100)]
        public string? DisplayName { get; set; }
    }
}
