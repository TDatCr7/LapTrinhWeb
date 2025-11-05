namespace QLRapPhim.Models
{
    public class BookingHistoryViewModel
    {
        public int BookingId { get; set; }
        public SuatChieu Showtime { get; set; }
        public string SeatPositions { get; set; }
        public int Quantity { get; set; }
        public DoAn? Food { get; set; }
        public List<BookingDetails> BookingDetails { get; set; }
        public decimal TotalTicketPrice { get; set; }
        public decimal TotalFoodPrice { get; set; }
        public decimal TotalPrice => TotalTicketPrice + TotalFoodPrice;
        public bool IsConfirmed { get; set; }
        public bool IsFailed { get; set; }
    }
}
