using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Pages.Teachers
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public CreateModel(SchoolDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public TeacherTb Teacher { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Ensure ID is default/0
            Teacher.TeachId = 0;

            _context.TeacherTbs.Add(Teacher);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Teacher created successfully.";
            return RedirectToPage("/Teachers");
        }
    }
}
