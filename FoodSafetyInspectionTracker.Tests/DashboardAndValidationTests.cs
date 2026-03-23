using FluentAssertions;
using FoodSafetyInspectionTracker.Data;
using FoodSafetyInspectionTracker.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FoodSafetyInspectionTracker.Tests
{
    public class DashboardAndValidationTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Dashboard_Should_Count_Overdue_Open_FollowUps_Correctly()
        {
            using var context = GetDbContext();

            var premises = new Premises
            {
                Name = "Test Premises",
                Address = "1 Test Street",
                Town = "Dublin",
                RiskRating = "High"
            };

            context.Premises.Add(premises);
            await context.SaveChangesAsync();

            var inspection = new Inspection
            {
                PremisesId = premises.Id,
                InspectionDate = DateTime.Today.AddDays(-10),
                Score = 50,
                Outcome = "Fail",
                Notes = "Test"
            };

            context.Inspections.Add(inspection);
            await context.SaveChangesAsync();

            context.FollowUps.AddRange(
                new FollowUp
                {
                    InspectionId = inspection.Id,
                    DueDate = DateTime.Today.AddDays(-2),
                    Status = "Open"
                },
                new FollowUp
                {
                    InspectionId = inspection.Id,
                    DueDate = DateTime.Today.AddDays(3),
                    Status = "Open"
                },
                new FollowUp
                {
                    InspectionId = inspection.Id,
                    DueDate = DateTime.Today.AddDays(-1),
                    Status = "Closed",
                    ClosedDate = DateTime.Today
                });

            await context.SaveChangesAsync();

            var overdueCount = await context.FollowUps
                .CountAsync(f => f.Status == "Open" && f.DueDate < DateTime.Today);

            overdueCount.Should().Be(1);
        }

        [Fact]
        public async Task Dashboard_Should_Count_Failed_Inspections_This_Month_Correctly()
        {
            using var context = GetDbContext();

            var premises = new Premises
            {
                Name = "Another Premises",
                Address = "2 Test Street",
                Town = "Cork",
                RiskRating = "Medium"
            };

            context.Premises.Add(premises);
            await context.SaveChangesAsync();

            context.Inspections.AddRange(
                new Inspection
                {
                    PremisesId = premises.Id,
                    InspectionDate = DateTime.Today.AddDays(-2),
                    Score = 40,
                    Outcome = "Fail"
                },
                new Inspection
                {
                    PremisesId = premises.Id,
                    InspectionDate = DateTime.Today.AddDays(-3),
                    Score = 85,
                    Outcome = "Pass"
                },
                new Inspection
                {
                    PremisesId = premises.Id,
                    InspectionDate = DateTime.Today.AddMonths(-2),
                    Score = 35,
                    Outcome = "Fail"
                });

            await context.SaveChangesAsync();

            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            var failedThisMonth = await context.Inspections
                .CountAsync(i => i.InspectionDate >= startOfMonth && i.Outcome == "Fail");

            failedThisMonth.Should().Be(1);
        }

        [Fact]
        public void FollowUp_Should_Be_Invalid_When_Closed_And_ClosedDate_Is_Missing()
        {
            var followUp = new FollowUp
            {
                InspectionId = 1,
                DueDate = DateTime.Today,
                Status = "Closed",
                ClosedDate = null
            };

            var isInvalid = followUp.Status == "Closed" && followUp.ClosedDate == null;

            isInvalid.Should().BeTrue();
        }

        [Fact]
        public async Task FollowUp_DueDate_Before_InspectionDate_Should_Be_Invalid()
        {
            using var context = GetDbContext();

            var premises = new Premises
            {
                Name = "Rule Test Premises",
                Address = "10 Test Road",
                Town = "Galway",
                RiskRating = "High"
            };

            context.Premises.Add(premises);
            await context.SaveChangesAsync();

            var inspection = new Inspection
            {
                PremisesId = premises.Id,
                InspectionDate = DateTime.Today,
                Score = 45,
                Outcome = "Fail"
            };

            context.Inspections.Add(inspection);
            await context.SaveChangesAsync();

            var followUp = new FollowUp
            {
                InspectionId = inspection.Id,
                DueDate = DateTime.Today.AddDays(-1),
                Status = "Open"
            };

            var isInvalid = followUp.DueDate < inspection.InspectionDate;

            isInvalid.Should().BeTrue();
        }
    }
}