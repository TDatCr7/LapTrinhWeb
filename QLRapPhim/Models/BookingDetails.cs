using System.ComponentModel.DataAnnotations;

namespace QLRapPhim.Models
{
    public class BookingDetails
    {
        [Key]
        public int BookingId { get; set; }
        public int TicketPriceId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public Booking Booking { get; set; }
        public GiaVe TicketPrice { get; set; }
    }
}
