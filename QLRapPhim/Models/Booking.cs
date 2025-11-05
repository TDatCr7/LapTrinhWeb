using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace QLRapPhim.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ShowtimeId { get; set; }
        public int SeatId { get; set; }
        public int? FoodId { get; set; }
        public DateTime BookingDate { get; set; }
        public bool IsConfirmed { get; set; } // Thêm trường này, mặc định false
        public bool IsFailed { get; set; }
        public SuatChieu Showtime { get; set; }
        public DoAn? Food { get; set; }
        public AppUser User { get; set; }
        public List<BookingDetails> BookingDetails { get; set; }
    }
}