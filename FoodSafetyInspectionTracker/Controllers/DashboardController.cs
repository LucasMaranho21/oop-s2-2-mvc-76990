using FoodSafetyInspectionTracker.Data;
using FoodSafetyInspectionTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FoodSafetyInspectionTracker.Controllers
{
    [Authorize(Roles = "Admin,Inspector,Viewer")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? town, string? riskRating)
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            IQueryable<Models.Inspection> inspectionsQuery = _context.Inspections
                .Include(i => i.Premises)
                .Where(i => i.InspectionDate >= startOfMonth);

            if (!string.IsNullOrWhiteSpace(town))
            {
                inspectionsQuery = inspectionsQuery.Where(i => i.Premises != null && i.Premises.Town == town);
            }

            if (!string.IsNullOrWhiteSpace(riskRating))
            {
                inspectionsQuery = inspectionsQuery.Where(i => i.Premises != null && i.Premises.RiskRating == riskRating);
            }

            IQueryable<Models.FollowUp> overdueFollowUpsQuery = _context.FollowUps
                .Include(f => f.Inspection)
                .ThenInclude(i => i!.Premises)
                .Where(f => f.Status == "Open" && f.DueDate < today);

            if (!string.IsNullOrWhiteSpace(town))
            {
                overdueFollowUpsQuery = overdueFollowUpsQuery.Where(f =>
                    f.Inspection != null &&
                    f.Inspection.Premises != null &&
                    f.Inspection.Premises.Town == town);
            }

            if (!string.IsNullOrWhiteSpace(riskRating))
            {
                overdueFollowUpsQuery = overdueFollowUpsQuery.Where(f =>
                    f.Inspection != null &&
                    f.Inspection.Premises != null &&
                    f.Inspection.Premises.RiskRating == riskRating);
            }

            var model = new DashboardViewModel
            {
                SelectedTown = town,
                SelectedRiskRating = riskRating,
                InspectionsThisMonth = await inspectionsQuery.CountAsync(),
                FailedInspectionsThisMonth = await inspectionsQuery.CountAsync(i => i.Outcome == "Fail"),
                OpenOverdueFollowUps = await overdueFollowUpsQuery.CountAsync(),
                Towns = await _context.Premises
                    .Select(p => p.Town)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync()
            };

            _logger.LogInformation(
                "Dashboard viewed with filters Town {Town} and RiskRating {RiskRating}",
                town ?? "All",
                riskRating ?? "All");

            return View(model);
        }
    }
}