using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EditModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string UserId { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            public List<string> SelectedRoles { get; set; } = new();

            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New Password (Optional)")]
            public string? NewPassword { get; set; }
        }

        public List<string> AllRoles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            UserId = user.Id;
            Input.Email = user.Email;

            // Get all roles
            AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            // Get user roles
            var userRoles = await _userManager.GetRolesAsync(user);
            Input.SelectedRoles = userRoles.ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            UserId = user.Id;
            AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            if (!ModelState.IsValid) return Page();

            // Update Email/Username
            if (Input.Email != user.Email)
            {
                user.Email = Input.Email;
                user.UserName = Input.Email; // Assuming username is email
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        return Page();
                    }
                }
            }

            // Password Reset Logic
            if (!string.IsNullOrEmpty(Input.NewPassword))
            {
                var removeResult = await _userManager.RemovePasswordAsync(user);
                if (removeResult.Succeeded)
                {
                    var addResult = await _userManager.AddPasswordAsync(user, Input.NewPassword);
                    if (!addResult.Succeeded)
                    {
                         foreach (var error in addResult.Errors)
                         {
                             ModelState.AddModelError(string.Empty, "Password Error: " + error.Description);
                             return Page();
                         }
                    }
                }
                else
                {
                     foreach (var error in removeResult.Errors)
                     {
                         ModelState.AddModelError(string.Empty, "Error removing old password: " + error.Description);
                         return Page();
                     }
                }
            }

            // Update Roles
            var userRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = Input.SelectedRoles ?? new List<string>();

            var rolesToAdd = selectedRoles.Except(userRoles);
            var rolesToRemove = userRoles.Except(selectedRoles);

            await _userManager.AddToRolesAsync(user, rolesToAdd);
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToPage("./Index");
        }
    }
}
