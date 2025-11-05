using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLRapPhim.Models
{
    public class PhongChieu
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        public List<SuatChieu> Showtimes { get; set; }
        public List<GheNgoi> Seats { get; set; }
    }
}
