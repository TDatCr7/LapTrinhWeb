using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLRapPhim.Models
{
    public class GheNgoi
    {
        public int Id { get; set; }
        public string Type { get; set; } // Loại ghế: thường, VIP, đặc biệt
        public string Position { get; set; }
        public int ScreenId { get; set; }
        public PhongChieu Screen { get; set; }
    }
}
