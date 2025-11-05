using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QLRapPhim.Models;

namespace QLRapPhim.Models
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<SuatChieu> SuatChieus { get; set; }
        public DbSet<PhongChieu> PhongChieus { get; set; }
        public DbSet<GheNgoi> GheNgois { get; set; }
        public DbSet<GiaVe> GiaVes { get; set; }
        public DbSet<DoAn> DoAns { get; set; }
        public DbSet<BookingDetails> BookingDetails { get; set; }
        public DbSet<Phim> Phims { get; set; }
        public DbSet<LoaiPhim> LoaiPhims { get; set; }

        public DbSet<SeatViewModel> SeatViewModels { get; set; }

    }
}