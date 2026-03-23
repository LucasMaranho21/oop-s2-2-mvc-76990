using FoodSafetyInspectionTracker.Data;
using FoodSafetyInspectionTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyInspectionTracker.Seeding
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            string[] roles = { "Admin", "Inspector", "Viewer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            await CreateUserIfMissing(userManager, "admin@foodsafety.local", "Admin123!", "Admin");
            await CreateUserIfMissing(userManager, "inspector@foodsafety.local", "Inspector123!", "Inspector");
            await CreateUserIfMissing(userManager, "viewer@foodsafety.local", "Viewer123!", "Viewer");

            if (await context.Premises.AnyAsync())
                return;

            var premisesList = new List<Premises>
            {
                new() { Name = "Oak Cafe", Address = "1 Main Street", Town = "Dublin", RiskRating = "Low" },
                new() { Name = "River Diner", Address = "2 Main Street", Town = "Dublin", RiskRating = "Medium" },
                new() { Name = "Green Fork", Address = "3 Main Street", Town = "Dublin", RiskRating = "High" },

                new() { Name = "Sunny Pizza", Address = "4 King Street", Town = "Cork", RiskRating = "Medium" },
                new() { Name = "Harbour Grill", Address = "5 King Street", Town = "Cork", RiskRating = "High" },
                new() { Name = "Market Bites", Address = "6 King Street", Town = "Cork", RiskRating = "Low" },

                new() { Name = "Hilltop Bakery", Address = "7 Bridge Road", Town = "Galway", RiskRating = "Low" },
                new() { Name = "West End Eatery", Address = "8 Bridge Road", Town = "Galway", RiskRating = "High" },
                new() { Name = "Fresh Bowl", Address = "9 Bridge Road", Town = "Galway", RiskRating = "Medium" }
            };

            context.Premises.AddRange(premisesList);
            await context.SaveChangesAsync();

            var inspections = new List<Inspection>
            {
                new() { PremisesId = premisesList[0].Id, InspectionDate = DateTime.Today.AddDays(-5), Score = 88, Outcome = "Pass", Notes = "Clean kitchen and good storage." },
                new() { PremisesId = premisesList[1].Id, InspectionDate = DateTime.Today.AddDays(-12), Score = 54, Outcome = "Fail", Notes = "Temperature logs incomplete." },
                new() { PremisesId = premisesList[2].Id, InspectionDate = DateTime.Today.AddDays(-18), Score = 61, Outcome = "Pass", Notes = "Minor hygiene issues corrected." },

                new() { PremisesId = premisesList[3].Id, InspectionDate = DateTime.Today.AddDays(-7), Score = 49, Outcome = "Fail", Notes = "Cross-contamination risk found." },
                new() { PremisesId = premisesList[4].Id, InspectionDate = DateTime.Today.AddDays(-20), Score = 91, Outcome = "Pass", Notes = "Excellent compliance." },
                new() { PremisesId = premisesList[5].Id, InspectionDate = DateTime.Today.AddDays(-2), Score = 75, Outcome = "Pass", Notes = "Generally good." },

                new() { PremisesId = premisesList[6].Id, InspectionDate = DateTime.Today.AddDays(-10), Score = 58, Outcome = "Fail", Notes = "Cleaning schedule missing." },
                new() { PremisesId = premisesList[7].Id, InspectionDate = DateTime.Today.AddDays(-15), Score = 95, Outcome = "Pass", Notes = "Excellent standards." },
                new() { PremisesId = premisesList[8].Id, InspectionDate = DateTime.Today.AddDays(-3), Score = 68, Outcome = "Pass", Notes = "Some paperwork issues." }
            };

            context.Inspections.AddRange(inspections);
            await context.SaveChangesAsync();

            var followUps = new List<FollowUp>
            {
                new() { InspectionId = inspections[1].Id, DueDate = DateTime.Today.AddDays(-2), Status = "Open" },
                new() { InspectionId = inspections[3].Id, DueDate = DateTime.Today.AddDays(5), Status = "Open" },
                new() { InspectionId = inspections[6].Id, DueDate = DateTime.Today.AddDays(-1), Status = "Closed", ClosedDate = DateTime.Today }
            };

            context.FollowUps.AddRange(followUps);
            await context.SaveChangesAsync();
        }

        private static async Task CreateUserIfMissing(
            UserManager<IdentityUser> userManager,
            string email,
            string password,
            string role)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    throw new Exception("Failed to create seeded user: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}