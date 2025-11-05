using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QLRapPhim.Models
{
    public class SeatViewModel
    {
        public int Id { get; set; }
        public string Position { get; set; }
        public bool IsOccupied { get; set; }
    }
}