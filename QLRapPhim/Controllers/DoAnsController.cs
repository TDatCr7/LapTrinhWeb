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
    public class DoAnsController : Controller
    {
        private readonly AppDbContext _context;

        public DoAnsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DoAns
        public async Task<IActionResult> Index()
        {
            return View(await _context.DoAns.ToListAsync());
        }

        // GET: DoAns/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doAn = await _context.DoAns
                .FirstOrDefaultAsync(m => m.Id == id);
            if (doAn == null)
            {
                return NotFound();
            }

            return View(doAn);
        }

        // GET: DoAns/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DoAns/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price")] DoAn doAn)
        {
            if (ModelState.IsValid)
            {
                _context.Add(doAn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(doAn);
        }

        // GET: DoAns/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doAn = await _context.DoAns.FindAsync(id);
            if (doAn == null)
            {
                return NotFound();
            }
            return View(doAn);
        }

        // POST: DoAns/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price")] DoAn doAn)
        {
            if (id != doAn.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doAn);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoAnExists(doAn.Id))
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
            return View(doAn);
        }

        // GET: DoAns/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doAn = await _context.DoAns
                .FirstOrDefaultAsync(m => m.Id == id);
            if (doAn == null)
            {
                return NotFound();
            }

            return View(doAn);
        }

        // POST: DoAns/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doAn = await _context.DoAns.FindAsync(id);
            if (doAn != null)
            {
                _context.DoAns.Remove(doAn);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DoAnExists(int id)
        {
            return _context.DoAns.Any(e => e.Id == id);
        }
    }
}
