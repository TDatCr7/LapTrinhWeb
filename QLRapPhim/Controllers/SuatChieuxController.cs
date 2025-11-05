using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLRapPhim.Models;

namespace QLRapPhim.Controllers
{
    public class SuatChieuxController : Controller
    {
        private readonly AppDbContext _context;

        public SuatChieuxController(AppDbContext context)
        {
            _context = context;
        }

        // GET: SuatChieux
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.SuatChieus.Include(s => s.Movie).Include(s => s.Screen);
            return View(await appDbContext.ToListAsync());
        }

        // GET: SuatChieux/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var suatChieu = await _context.SuatChieus
                .Include(s => s.Movie)
                .Include(s => s.Screen)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (suatChieu == null)
            {
                return NotFound();
            }

            return View(suatChieu);
        }

        // GET: SuatChieux/Create
        public IActionResult Create()
        {
            ViewData["MovieId"] = new SelectList(_context.Phims, "Id", "Name");
            ViewData["ScreenId"] = new SelectList(_context.PhongChieus, "Id", "Name");
            return View();
        }

        // POST: SuatChieux/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MovieId,ScreenId,Format,ShowDate,StartTime")] SuatChieu suatChieu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(suatChieu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MovieId"] = new SelectList(_context.Phims, "Id", "Name", suatChieu.MovieId);
            ViewData["ScreenId"] = new SelectList(_context.PhongChieus, "Id", "Name", suatChieu.ScreenId);
            return View(suatChieu);
        }

        // GET: SuatChieux/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var suatChieu = await _context.SuatChieus.FindAsync(id);
            if (suatChieu == null)
            {
                return NotFound();
            }
            ViewData["MovieId"] = new SelectList(_context.Phims, "Id", "Name", suatChieu.MovieId);
            ViewData["ScreenId"] = new SelectList(_context.PhongChieus, "Id", "Name", suatChieu.ScreenId);
            return View(suatChieu);
        }

        // POST: SuatChieux/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MovieId,ScreenId,Format,ShowDate,StartTime")] SuatChieu suatChieu)
        {
            if (id != suatChieu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(suatChieu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SuatChieuExists(suatChieu.Id))
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
            ViewData["MovieId"] = new SelectList(_context.Phims, "Id", "Name", suatChieu.MovieId);
            ViewData["ScreenId"] = new SelectList(_context.PhongChieus, "Id", "Name", suatChieu.ScreenId);
            return View(suatChieu);
        }

        // GET: SuatChieux/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var suatChieu = await _context.SuatChieus
                .Include(s => s.Movie)
                .Include(s => s.Screen)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (suatChieu == null)
            {
                return NotFound();
            }

            return View(suatChieu);
        }

        // POST: SuatChieux/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var suatChieu = await _context.SuatChieus.FindAsync(id);
            if (suatChieu != null)
            {
                _context.SuatChieus.Remove(suatChieu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SuatChieuExists(int id)
        {
            return _context.SuatChieus.Any(e => e.Id == id);
        }
    }
}
