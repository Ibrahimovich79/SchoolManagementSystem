using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly SchoolDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CreateModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SchoolDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public SelectList Roles { get; set; } = default!;

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = default!;

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = default!;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = default!;

            [Required]
            [Display(Name = "Role")]
            public string Role { get; set; } = default!;

            [Display(Name = "Teacher")]
            public long? TeacherId { get; set; }

            [Display(Name = "Student")]
            public long? StudentId { get; set; }
        }

        public async Task OnGetAsync()
        {
            await PopulateDropdowns();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Custom Validation
            if ((Input.Role == "Teacher" || Input.Role == "Supervisor") && Input.TeacherId == null)
            {
                ModelState.AddModelError("Input.TeacherId", $"Please select a teacher when creating a {Input.Role} account.");
            }
            if (Input.Role == "Student" && Input.StudentId == null)
            {
                ModelState.AddModelError("Input.StudentId", "Please select a student when creating a Student account.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return Page();
            }

            var user = new IdentityUser { UserName = Input.Email, Email = Input.Email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(Input.Role))
                {
                    await _userManager.AddToRoleAsync(user, Input.Role);

                    // Link Teacher or Supervisor to TeacherTb
                    if ((Input.Role == "Teacher" || Input.Role == "Supervisor") && Input.TeacherId.HasValue)
                    {
                        var teacher = await _context.TeacherTbs.FindAsync(Input.TeacherId.Value);
                        if (teacher != null)
                        {
                            teacher.UserId = user.Id;
                            _context.TeacherTbs.Update(teacher);
                            await _context.SaveChangesAsync();
                        }
                    }
                    // Link Student to StdTable
                    else if (Input.Role == "Student" && Input.StudentId.HasValue)
                    {
                        var student = await _context.StdTables.FindAsync(Input.StudentId.Value);
                        if (student != null)
                        {
                            student.UserId = user.Id;
                            _context.StdTables.Update(student);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                return RedirectToPage("./Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await PopulateDropdowns();
            return Page();
        }

        private async Task PopulateDropdowns()
        {
            // Roles
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            Roles = new SelectList(roles);

            // Unlinked Teachers (for Teacher/Supervisor)
            var unlinkedTeachers = await _context.TeacherTbs
                .Where(t => t.UserId == null)
                .Select(t => new { t.TeachId, t.TeachName })
                .ToListAsync();
            ViewData["Teachers"] = new SelectList(unlinkedTeachers, "TeachId", "TeachName");

            // Unlinked Students (for Student)
            var unlinkedStudents = await _context.StdTables
                .Where(s => s.UserId == null)
                .Select(s => new { QID = s.Qid, Name = s.StdName + " (Code: " + s.StdCode + ")" })
                .ToListAsync();
            ViewData["Students"] = new SelectList(unlinkedStudents, "QID", "Name");
        }
    }
}
