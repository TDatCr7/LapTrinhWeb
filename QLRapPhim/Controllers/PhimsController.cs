using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLRapPhim.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QLRapPhim.Controllers
{
    [Authorize(Roles = SD.Role_Admin)] // Giả sử bạn có SD.Role_Admin tương tự LoaiPhimsController
    public class PhimsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PhimsController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Phims
        // GET: Phims
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5, string keyword = null, int? categoryId = null)
        {
            var query = _context.Phims.Include(p => p.Category).AsQueryable();

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.Name.Contains(keyword));
            }

            // Lọc theo thể loại
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Tính tổng số phim
            var totalItems = await query.CountAsync();

            // Lấy danh sách phim với phân trang
            var phims = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền thông tin phân trang và danh sách thể loại vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.Keyword = keyword;
            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = new SelectList(_context.LoaiPhims, "Id", "Name", categoryId);

            return View(phims);
        }

        // GET: Phims/Search
        [HttpGet]
        public async Task<IActionResult> Search(string keyword, int? categoryId)
        {
            return await Index(1, 5, keyword, categoryId);
        }

        // GET: Phims/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phim = await _context.Phims
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (phim == null)
            {
                return NotFound();
            }

            return View(phim);
        }

        // GET: Phims/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.LoaiPhims, "Id", "Name");
            return View();
        }

        // POST: Phims/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Duration,ReleaseDate,CategoryId,ImageUrl,Description")] Phim phim)
        {
            if (ModelState.IsValid)
            {
                _context.Add(phim);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.LoaiPhims, "Id", "Name", phim.CategoryId);
            return View(phim);
        }

        // GET: Phims/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phim = await _context.Phims.FindAsync(id);
            if (phim == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.LoaiPhims, "Id", "Name", phim.CategoryId);
            return View(phim);
        }

        // POST: Phims/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Duration,ReleaseDate,CategoryId,ImageUrl,Description")] Phim phim, IFormFile imageFile)
        {
            if (id != phim.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPhim = await _context.Phims.FindAsync(id);
                    if (existingPhim == null)
                        return NotFound();

                    existingPhim.Name = phim.Name;
                    existingPhim.Duration = phim.Duration;
                    existingPhim.ReleaseDate = phim.ReleaseDate;
                    existingPhim.CategoryId = phim.CategoryId;
                    existingPhim.Description = phim.Description;

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        existingPhim.ImageUrl = "/images/" + uniqueFileName;
                    }

                    _context.Update(existingPhim);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Phims.Any(e => e.Id == phim.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewData["CategoryId"] = new SelectList(_context.LoaiPhims, "Id", "Name", phim.CategoryId);
            return View(phim);
        }

        // GET: Phims/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phim = await _context.Phims
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (phim == null)
            {
                return NotFound();
            }

            return View(phim);
        }

        // POST: Phims/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phim = await _context.Phims.FindAsync(id);
            if (phim != null)
            {
                _context.Phims.Remove(phim);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhimExists(int id)
        {
            return _context.Phims.Any(e => e.Id == id);
        }
    }
}