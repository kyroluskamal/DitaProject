using AppConfgDocumentation.Data;
using AppConfgDocumentation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppConfgDocumentation.Data
{
    public class DbContextInitializer(ApplicationDbContext applicationDbContext,
     UserManager<ApplicationUser> userManager,
     RoleManager<IdentityRole<int>> roleManager) : IDbContextInitializer
    {
        private readonly ApplicationDbContext ApplicationDbContext = applicationDbContext;
        private readonly UserManager<ApplicationUser> UserManager = userManager;
        private readonly RoleManager<IdentityRole<int>> RoleManager = roleManager;

        public async Task Initialize()
        {

            // var trasnaction = ApplicationDbContext.Database.BeginTransaction();
            try
            {
                await createAnalystAccount();
                await createDeveloperAccount();
                await createTechnicianAccount();
                // trasnaction.Commit();
            }
            catch (Exception ex)
            {
                // trasnaction.Rollback();
                throw ex;
            }
        }

        private async Task createAnalystAccount()
        {
            var newUser = new ApplicationUser
            {
                UserName = "Analyst",
                Email = "admin@dita.com",
                FirstName = "Kyrolus",
                LastName = "Sous",
                EmailConfirmed = true,
            };
            var user = await UserManager.FindByEmailAsync(newUser.Email);
            if (user != null)
            {
                return;
            }
            var result = await UserManager.CreateAsync(newUser, "admin@123");
            if (result.Succeeded)
            {
                var role = new IdentityRole<int>
                {
                    Name = "Analyst"
                };
                var roleExists = await RoleManager.RoleExistsAsync("Analyst");
                if (!roleExists)
                    await RoleManager.CreateAsync(role);
                await UserManager.AddToRoleAsync(newUser, "Analyst");
                await UserManager.UpdateSecurityStampAsync(newUser);
            }

        }

        private async Task createDeveloperAccount()
        {
            var newUser = new ApplicationUser
            {
                UserName = "Developer",
                Email = "developer@dita.com",
                FirstName = "Sabri",
                LastName = "Manai",
                EmailConfirmed = true,
            };
            var user = await UserManager.FindByEmailAsync(newUser.Email);
            if (user == null)
            {
                var result = await UserManager.CreateAsync(newUser, "developer@123");
                if (result.Succeeded)
                {
                    var role = new IdentityRole<int>
                    {
                        Name = "Developer"
                    };
                    var roleExists = await RoleManager.RoleExistsAsync("Developer");
                    if (!roleExists)
                        await RoleManager.CreateAsync(role);
                    await UserManager.AddToRoleAsync(newUser, "Developer");
                }
                await UserManager.UpdateSecurityStampAsync(newUser);

            }
        }

        private async Task createTechnicianAccount()
        {
            var newUser = new ApplicationUser
            {
                UserName = "Technician",
                Email = "tech@dita.com",
                FirstName = "Mohamed",
                LastName = "Salem",
                EmailConfirmed = true,
            };
            var user = await UserManager.FindByEmailAsync(newUser.Email);

            if (user != null)
            {
                return;
            }

            var result = await UserManager.CreateAsync(newUser, "tech@123");
            if (result.Succeeded)
            {
                var role = new IdentityRole<int>
                {
                    Name = "Technician"
                };
                var roleExists = await RoleManager.RoleExistsAsync("Technician");
                if (!roleExists)
                    await RoleManager.CreateAsync(role);
                await UserManager.AddToRoleAsync(newUser, "Technician");
                await UserManager.UpdateSecurityStampAsync(newUser);
            }

        }
    }
}
