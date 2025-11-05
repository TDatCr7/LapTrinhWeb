using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLRapPhim.Models
{
    public class Phim
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        [Required]
        public int Duration { get; set; }
        [Required]
        public DateTime ReleaseDate { get; set; }
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public LoaiPhim? Category { get; set; }
        [StringLength(250)]
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public List<SuatChieu>? Showtimes { get; set; }
    }
}
