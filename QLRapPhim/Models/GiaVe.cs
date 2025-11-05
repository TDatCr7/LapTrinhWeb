using System.ComponentModel.DataAnnotations;

namespace QLRapPhim.Models
{
    public class GiaVe
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        [Required, Range(20000, 200000)]
        public decimal Price { get; set; }
        public List<BookingDetails> BookingDetails { get; set; }
    }
}
