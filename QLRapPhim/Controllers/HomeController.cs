using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLRapPhim.Models;

namespace QLRapPhim.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string keyword = "", int categoryId = 0)
        {
            var movies = _context.Phims.Include(p => p.Category).Include(p => p.Showtimes).AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                movies = movies.Where(m => m.Name.Contains(keyword));
            }

            if (categoryId > 0)
            {
                movies = movies.Where(m => m.CategoryId == categoryId);
            }

            int pageSize = 8;
            var totalMovies = await movies.CountAsync();
            var pagedMovies = await movies
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalMovies / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.LoaiPhims = await _context.LoaiPhims.ToListAsync();
            ViewBag.Keyword = keyword;
            ViewBag.CategoryId = categoryId;

            return View(pagedMovies);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var movie = await _context.Phims
                .Include(p => p.Category)
                .Include(p => p.Showtimes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        public IActionResult Search(string keyword, int categoryId)
        {
            return RedirectToAction("Index", new { keyword, categoryId });
        }
    }
}