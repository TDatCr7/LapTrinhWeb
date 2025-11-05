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
    public class GheNgoisController : Controller
    {
        private readonly AppDbContext _context;

        public GheNgoisController(AppDbContext context)
        {
            _context = context;
        }

        // GET: GheNgois
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.GheNgois.Include(g => g.Screen);
            return View(await appDbContext.ToListAsync());
        }

        // GET: GheNgois/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gheNgoi = await _context.GheNgois
                .Include(g => g.Screen)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gheNgoi == null)
            {
                return NotFound();
            }

            return View(gheNgoi);
        }

        // GET: GheNgois/Create
        public IActionResult Create()
        {
            ViewData["ScreenId"] = new SelectList(_context.PhongChieus, "Id", "Name");
            return View();
        }

        // POST: GheNgois/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type,Position,ScreenId")] GheNgoi gheNgoi)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gheNgoi);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ScreenId"] = new SelectList(_context.PhongChieus, "Id", "Name", gheNgoi.ScreenId);
            return View(gheNgoi);
        }

        // GET: GheNgois/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gheNgoi = await _context.GheNgois.FindAsync(id);
            if (gheNgoi == null)
            {
                return NotFound();
            }
            ViewData["ScreenId"] = new SelectList(_context.PhongChieus, "Id", "Name", gheNgoi.ScreenId);
            return View(gheNgoi);
        }

        // POST: GheNgois/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Type,Position,ScreenId")] GheNgoi gheNgoi)
        {
            if (id != gheNgoi.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gheNgoi);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GheNgoiExists(gheNgoi.Id))
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
            ViewData["ScreenId"] = new SelectList(_context.PhongChieus, "Id", "Name", gheNgoi.ScreenId);
            return View(gheNgoi);
        }

        // GET: GheNgois/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gheNgoi = await _context.GheNgois
                .Include(g => g.Screen)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gheNgoi == null)
            {
                return NotFound();
            }

            return View(gheNgoi);
        }

        // POST: GheNgois/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gheNgoi = await _context.GheNgois.FindAsync(id);
            if (gheNgoi != null)
            {
                _context.GheNgois.Remove(gheNgoi);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GheNgoiExists(int id)
        {
            return _context.GheNgois.Any(e => e.Id == id);
        }
    }
}
