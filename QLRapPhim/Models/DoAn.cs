using System.ComponentModel.DataAnnotations;

namespace QLRapPhim.Models
{
    public class DoAn
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        [Required, Range(10000, 200000)]
        public decimal Price { get; set; }
        public List<Booking> Bookings { get; set; }
    }
}
