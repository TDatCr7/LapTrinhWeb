using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLRapPhim.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QLRapPhim.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class LoaiPhimsController : Controller
    {
        private readonly AppDbContext _context;

        public LoaiPhimsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: LoaiPhims
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5, string keyword = null)
        {
            var query = _context.LoaiPhims.AsQueryable();

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(lp => lp.Name.Contains(keyword));
            }

            // Tính tổng số thể loại
            var totalItems = await query.CountAsync();

            // Lấy danh sách thể loại với phân trang
            var categories = await query
                .OrderBy(lp => lp.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền thông tin phân trang vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.Keyword = keyword;

            return View(categories);
        }

        // GET: LoaiPhims/Search
        [HttpGet]
        public async Task<IActionResult> Search(string keyword)
        {
            return await Index(1, 5, keyword);
        }

        // GET: LoaiPhims/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiPhim = await _context.LoaiPhims
                .FirstOrDefaultAsync(m => m.Id == id);
            if (loaiPhim == null)
            {
                return NotFound();
            }

            return View(loaiPhim);
        }

        // GET: LoaiPhims/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LoaiPhims/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] LoaiPhim loaiPhim)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loaiPhim);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(loaiPhim);
        }

        // GET: LoaiPhims/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiPhim = await _context.LoaiPhims.FindAsync(id);
            if (loaiPhim == null)
            {
                return NotFound();
            }
            return View(loaiPhim);
        }

        // POST: LoaiPhims/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] LoaiPhim loaiPhim)
        {
            if (id != loaiPhim.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loaiPhim);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoaiPhimExists(loaiPhim.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(loaiPhim);
        }

        // GET: LoaiPhims/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiPhim = await _context.LoaiPhims
                .FirstOrDefaultAsync(m => m.Id == id);
            if (loaiPhim == null)
            {
                return NotFound();
            }

            return View(loaiPhim);
        }

        // POST: LoaiPhims/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loaiPhim = await _context.LoaiPhims.FindAsync(id);
            if (loaiPhim != null)
            {
                _context.LoaiPhims.Remove(loaiPhim);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LoaiPhimExists(int id)
        {
            return _context.LoaiPhims.Any(e => e.Id == id);
        }
    }
}