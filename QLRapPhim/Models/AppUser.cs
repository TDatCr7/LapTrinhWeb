using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace QLRapPhim.Models
{
    public class AppUser : IdentityUser
    {
        [Required, StringLength(50)]
        public string FullName { get; set; }
        [StringLength(100)]
        public string? Address { get; set; }
        [StringLength(10)]
        public string? Age { get; set; }
        public List<Booking> Bookings { get; set; }
    }
}
