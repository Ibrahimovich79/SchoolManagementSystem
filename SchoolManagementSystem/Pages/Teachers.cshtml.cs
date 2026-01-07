using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
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

        public async Task OnGetAsync(string sortOrder, string searchString)
        {
            CurrentSort = sortOrder;
            IdSort = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            NameSort = sortOrder == "Name" ? "name_desc" : "Name";
            
            ViewData["CurrentFilter"] = searchString;

            IQueryable<TeacherTb> query = _context.TeacherTbs
                .Include(t => t.Cousre)
                .Include(t => t.Relations)
                .ThenInclude(r => r.GradeNavigation);

            if (!string.IsNullOrEmpty(searchString))
            {
                if (long.TryParse(searchString, out long searchId))
                {
                    query = query.Where(t => t.TeachId == searchId || t.TeachName.Contains(searchString));
                }
                else
                {
                    query = query.Where(t => t.TeachName.Contains(searchString));
                }
            }

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

            // Access Control: Non-Admin/Non-Supervisor sees ONLY their own record
            if (!User.IsInRole("Admin") && !User.IsInRole("Supervisor"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var linkedTeacher = await _context.TeacherTbs.FirstOrDefaultAsync(t => t.UserId == userId);

                if (linkedTeacher == null)
                {
                    TempData["ErrorMessage"] = "Your account is not linked to a teacher profile. Please contact admin.";
                    Teachers = new List<TeacherTb>();
                    return;
                }

                query = query.Where(t => t.UserId == userId);
            }

            Teachers = await query.ToListAsync();

            // Server-Side Data Masking for non-Admin/non-Supervisor
            if (!User.IsInRole("Admin") && !User.IsInRole("Supervisor"))
            {
                foreach (var teacher in Teachers)
                {
                    teacher.TeachMobile = null;
                    teacher.StaffId = null;
                }
            }
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