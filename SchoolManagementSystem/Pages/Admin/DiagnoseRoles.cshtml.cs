using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Pages.Admin
{
    public class DiagnoseRolesModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DiagnoseRolesModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IdentityUser TargetUser { get; set; }
        public IList<string> TargetUserRoles { get; set; } = new List<string>();
        public List<IdentityRole> AllRoles { get; set; } = new List<IdentityRole>();

        public async Task OnGetAsync()
        {
            AllRoles = await _roleManager.Roles.ToListAsync();

            var adminEmail = "admin@school.com";
            TargetUser = await _userManager.FindByEmailAsync(adminEmail);

            if (TargetUser != null)
            {
                TargetUserRoles = await _userManager.GetRolesAsync(TargetUser);
            }
        }
    }
}
