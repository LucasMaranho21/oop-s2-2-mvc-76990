using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FoodSafetyInspectionTracker.Data;
using FoodSafetyInspectionTracker.Models;

namespace FoodSafetyInspectionTracker.Controllers
{
    [Authorize(Roles = "Admin,Inspector,Viewer")]
    public class FollowUpsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FollowUpsController> _logger;

        public FollowUpsController(ApplicationDbContext context, ILogger<FollowUpsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: FollowUps
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.FollowUps.Include(f => f.Inspection);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: FollowUps/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null)
            {
                return NotFound();
            }

            return View(followUp);
        }

        // GET: FollowUps/Create
        [Authorize(Roles = "Admin,Inspector")]
        public IActionResult Create()
        {
            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id");
            return View();
        }

        // POST: FollowUps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create([Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            var inspection = await _context.Inspections.FindAsync(followUp.InspectionId);

            if (inspection == null)
            {
                ModelState.AddModelError("InspectionId", "Selected inspection was not found.");
                _logger.LogWarning(
                    "FollowUp creation failed because InspectionId {InspectionId} was not found",
                    followUp.InspectionId);
            }
            else if (followUp.DueDate < inspection.InspectionDate)
            {
                ModelState.AddModelError("DueDate", "Due date cannot be before the inspection date.");
                _logger.LogWarning(
                    "FollowUp creation failed because DueDate {DueDate} was before InspectionDate for InspectionId {InspectionId}",
                    followUp.DueDate,
                    followUp.InspectionId);
            }

            if (followUp.Status == "Closed" && followUp.ClosedDate == null)
            {
                ModelState.AddModelError("ClosedDate", "ClosedDate is required when the follow-up status is Closed.");
                _logger.LogWarning(
                    "FollowUp creation failed because ClosedDate was missing for closed FollowUp");
            }

            if (ModelState.IsValid)
            {
                _context.Add(followUp);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "FollowUp created with Id {FollowUpId} for InspectionId {InspectionId}",
                    followUp.Id,
                    followUp.InspectionId);

                return RedirectToAction(nameof(Index));
            }

            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", followUp.InspectionId);
            return View(followUp);
        }

        // GET: FollowUps/Edit/5
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp == null)
            {
                return NotFound();
            }

            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", followUp.InspectionId);
            return View(followUp);
        }

        // POST: FollowUps/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            if (id != followUp.Id)
            {
                return NotFound();
            }

            var inspection = await _context.Inspections.FindAsync(followUp.InspectionId);

            if (inspection == null)
            {
                ModelState.AddModelError("InspectionId", "Selected inspection was not found.");
                _logger.LogWarning(
                    "FollowUp update failed because InspectionId {InspectionId} was not found",
                    followUp.InspectionId);
            }
            else if (followUp.DueDate < inspection.InspectionDate)
            {
                ModelState.AddModelError("DueDate", "Due date cannot be before the inspection date.");
                _logger.LogWarning(
                    "FollowUp update failed because DueDate {DueDate} was before InspectionDate for InspectionId {InspectionId}",
                    followUp.DueDate,
                    followUp.InspectionId);
            }

            if (followUp.Status == "Closed" && followUp.ClosedDate == null)
            {
                ModelState.AddModelError("ClosedDate", "ClosedDate is required when the follow-up status is Closed.");
                _logger.LogWarning(
                    "FollowUp update failed because ClosedDate was missing for closed FollowUp");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(followUp);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "FollowUp updated with Id {FollowUpId}",
                        followUp.Id);

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FollowUpExists(followUp.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }

            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", followUp.InspectionId);
            return View(followUp);
        }

        // GET: FollowUps/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null)
            {
                return NotFound();
            }

            return View(followUp);
        }

        // POST: FollowUps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp != null)
            {
                _context.FollowUps.Remove(followUp);
                await _context.SaveChangesAsync();

                _logger.LogWarning(
                    "FollowUp deleted with Id {FollowUpId}",
                    id);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool FollowUpExists(int id)
        {
            return _context.FollowUps.Any(e => e.Id == id);
        }
    }
}