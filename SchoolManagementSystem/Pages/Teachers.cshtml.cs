using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Pages
{
    [Authorize]
    public class TeachersModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public TeachersModel(SchoolDbContext context)
        {
            _context = context;
        }

        public string CurrentSort { get; set; } = default!;
        public string IdSort { get; set; } = default!;
        public string NameSort { get; set; } = default!;

        public IList<TeacherTb> Teachers { get; set; } = default!;

        public async Task OnGetAsync(string sortOrder)
        {
            CurrentSort = sortOrder;
            IdSort = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            NameSort = sortOrder == "Name" ? "name_desc" : "Name";

            IQueryable<TeacherTb> query = _context.TeacherTbs;

            switch (sortOrder)
            {
                case "id_desc":
                    query = query.OrderByDescending(t => t.TeachId);
                    break;
                case "Name":
                    query = query.OrderBy(t => t.TeachName);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(t => t.TeachName);
                    break;
                default:
                    query = query.OrderBy(t => t.TeachId);
                    break;
            }

            Teachers = await query.ToListAsync();
        }
        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var teacher = await _context.TeacherTbs.FindAsync(id);

            if (teacher != null)
            {
                try
                {
                    _context.TeacherTbs.Remove(teacher);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Teacher deleted successfully.";
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "Cannot delete teacher. They may be linked to courses or other records.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Teacher not found.";
            }

            return RedirectToPage();
        }
    }
}