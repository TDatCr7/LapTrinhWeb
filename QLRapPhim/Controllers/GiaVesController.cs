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
    public class GiaVesController : Controller
    {
        private readonly AppDbContext _context;

        public GiaVesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: GiaVes
        public async Task<IActionResult> Index()
        {
            return View(await _context.GiaVes.ToListAsync());
        }

        // GET: GiaVes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var giaVe = await _context.GiaVes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (giaVe == null)
            {
                return NotFound();
            }

            return View(giaVe);
        }

        // GET: GiaVes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GiaVes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price")] GiaVe giaVe)
        {
            if (ModelState.IsValid)
            {
                _context.Add(giaVe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(giaVe);
        }

        // GET: GiaVes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var giaVe = await _context.GiaVes.FindAsync(id);
            if (giaVe == null)
            {
                return NotFound();
            }
            return View(giaVe);
        }

        // POST: GiaVes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price")] GiaVe giaVe)
        {
            if (id != giaVe.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(giaVe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GiaVeExists(giaVe.Id))
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
            return View(giaVe);
        }

        // GET: GiaVes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var giaVe = await _context.GiaVes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (giaVe == null)
            {
                return NotFound();
            }

            return View(giaVe);
        }

        // POST: GiaVes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var giaVe = await _context.GiaVes.FindAsync(id);
            if (giaVe != null)
            {
                _context.GiaVes.Remove(giaVe);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GiaVeExists(int id)
        {
            return _context.GiaVes.Any(e => e.Id == id);
        }
    }
}
