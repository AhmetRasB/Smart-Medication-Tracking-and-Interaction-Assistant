using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SMTIA.Domain.Entities;
using SMTIA.Infrastructure.Context;

namespace SMTIA.WebAPI.Middlewares
{
    public static class ExtensionsMiddleware
    {
        public static void CreateFirstUser(WebApplication app)
        {
            using (var scoped = app.Services.CreateScope())
            {
                var userManager = scoped.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var roleManager = scoped.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
                var db = scoped.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Ensure roles exist (only Admin + User)
                EnsureRoleExists(roleManager, "Admin").Wait();
                EnsureRoleExists(roleManager, "User").Wait();

                if (!userManager.Users.Any(p => p.UserName == "admin"))
                {
                    AppUser user = new()
                    {
                        UserName = "admin",
                        Email = "admin@admin.com",
                        FirstName = "Taner",
                        LastName = "Saydam",
                        EmailConfirmed = true
                    };

                    var createResult = userManager.CreateAsync(user, "1").Result;
                    if (createResult.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Admin").Wait();
                    }
                }
                else
                {
                    var adminUser = userManager.Users.First(p => p.UserName == "admin");
                    var roles = userManager.GetRolesAsync(adminUser).Result;
                    if (!roles.Contains("Admin"))
                    {
                        userManager.AddToRoleAsync(adminUser, "Admin").Wait();
                    }
                }

                // Seed dummy medicines for search/autocomplete if empty
                SeedDummyMedicines(db).Wait();
            }
        }

        private static async Task EnsureRoleExists(RoleManager<IdentityRole<Guid>> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }

        private static async Task SeedDummyMedicines(ApplicationDbContext db)
        {
            // If there are already medicines, do nothing
            if (await db.Medicines.AnyAsync(m => !m.IsDeleted))
                return;

            var now = DateTime.UtcNow;

            // 100 dummy Turkish-ish medicine names for onboarding search UX
            var names = new List<string>
            {
                "Parol","Aferin","Majezik","Dolorex","Vermidon","Minoset","Calpol","Nurofen","Apranax","Arveles",
                "Voltaren","Brufen","Cataflam","Buscopan","Rennie","Gaviscon","Nexium","Lansor","Panto","Rennie Duo",
                "Augmentin","Klamoks","Bactrim","Cefaks","Zinnat","Zitromax","Macrol","Suprax","Monodoks","Keflex",
                "Claritin","Aerius","Zyrtec","Xyzal","Telfast","Avil","Allerset","Desa","Lorin","Histop",
                "Ventolin","Symbicort","Seretide","Pulmicort","Asmanex","Atrovent","Berodual","Otrivine","Rinofluimucil","Sterimar",
                "Coraspin","Aspirin","Plavix","Ecopirin","Beloc","Concor","Norvasc","Cozaar","Micardis","Coverex",
                "Glifor","Glucophage","Januvia","Amaryl","Diamicron","Lantus","Novorapid","Humalog","Actrapid","Tresiba",
                "Dexpass","Nasonex","Avamys","Bepanthol","Fucidin","Madecassol","Sudocrem","Bepanthen","Terramycin","Mupirocin",
                "Vicks","Tylolhot","Iburamin","Theraflu","Coldaway","Gripin","Benical","Sinecod","Bronchipret","Mucovit-C",
                "Magnezyum","Berocca","Supradyn","Redoxon","Ocean Plus","D3K2","Vitabiotics","Balık Yağı","Zinco","Demir Plus"
            };

            var items = names.Select((n, idx) => new Medicine
            {
                Name = n,
                ActiveIngredient = null,
                DosageForm = (idx % 4) switch { 0 => "pill", 1 => "capsule", 2 => "bottle", _ => "syringe" },
                Manufacturer = null,
                CreatedAt = now
            }).ToList();

            db.Medicines.AddRange(items);
            await db.SaveChangesAsync();
        }
    }
}
