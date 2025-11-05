using System.ComponentModel.DataAnnotations;

namespace QLRapPhim.Models
{
    public class LoaiPhim
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        public List<Phim>? Movies { get; set; }
    }
}
