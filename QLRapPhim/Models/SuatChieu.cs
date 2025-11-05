using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLRapPhim.Models
{
    public class SuatChieu
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public int ScreenId { get; set; }
        [Required, StringLength(10)]
        public string Format { get; set; }
        [Required]
        public DateTime ShowDate { get; set; }
        [Required]
        public TimeSpan StartTime { get; set; }
        [ForeignKey("MovieId")]
        public Phim Movie { get; set; }
        [ForeignKey("ScreenId")]
        public PhongChieu Screen { get; set; }
        public List<Booking> Bookings { get; set; }
    }
}
