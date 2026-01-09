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
    public class StudentsModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public StudentsModel(SchoolDbContext context)
        {
            _context = context;
        }

        public string CurrentSort { get; set; } = default!;
        public string IdSort { get; set; } = default!;
        public string NameSort { get; set; } = default!;
        public string GradeSort { get; set; } = default!;
        public string ClassSort { get; set; } = default!;

        public string? CurrentFilter { get; set; }
        public string? CurrentGradeFilter { get; set; }

        public IList<StdTable> Students { get; set; } = default!;

        public async Task OnGetAsync(string sortOrder, string searchString, string? gradeId)
        {
            CurrentSort = sortOrder;
            CurrentFilter = searchString;
            CurrentGradeFilter = gradeId;

            IdSort = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            NameSort = sortOrder == "Name" ? "name_desc" : "Name";
            GradeSort = sortOrder == "Grade" ? "grade_desc" : "Grade";
            ClassSort = ""; // Deprecated

            IQueryable<StdTable> query = _context.StdTables.Include(s => s.StdGradeNavigation);

            if (!string.IsNullOrEmpty(searchString))
            {
                // Search by Name or ID
                var isNumeric = long.TryParse(searchString, out long searchId);
                if (isNumeric)
                {
                     query = query.Where(s => s.Qid == searchId || s.StdName.Contains(searchString));
                }
                else
                {
                    query = query.Where(s => s.StdName.Contains(searchString));
                }
            }

            if (!string.IsNullOrEmpty(gradeId))
            {
                query = query.Where(s => s.GradeId == gradeId);
            }

            switch (sortOrder)
            {
                case "id_desc":
                    query = query.OrderByDescending(s => s.Qid);
                    break;
                case "Name":
                    query = query.OrderBy(s => s.StdName);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(s => s.StdName);
                    break;
                case "Grade":
                    query = query.OrderBy(s => s.StdGradeNavigation != null ? s.StdGradeNavigation.GradeName : string.Empty);
                    break;
                case "grade_desc":
                    query = query.OrderByDescending(s => s.StdGradeNavigation != null ? s.StdGradeNavigation.GradeName : string.Empty);
                    break;
                default:
                    // Default: Sort by Class (Home Students and المنازل last), then by Name alphabetically
                    query = query
                        .OrderBy(s => s.StdGradeNavigation != null && s.StdGradeNavigation.GradeName != null && 
                            (s.StdGradeNavigation.GradeName.Contains("Home") || s.StdGradeNavigation.GradeName.Contains("المنازل")) ? 1 : 0)
                        .ThenBy(s => s.StdGradeNavigation != null ? s.StdGradeNavigation.GradeName : string.Empty)
                        .ThenBy(s => s.StdName);
                    break;
            }

            // Access Control: Teacher sees ONLY students in assigned grades
            if (!User.IsInRole("Admin") && !User.IsInRole("Supervisor"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var linkedTeacher = await _context.TeacherTbs
                    .Include(t => t.Relations)
                    .FirstOrDefaultAsync(t => t.UserId == userId);

                if (linkedTeacher == null)
                {
                    TempData["ErrorMessage"] = "Your account is not linked to a teacher profile. Please contact admin.";
                    Students = new List<StdTable>();
                    return; // Stop processing
                }

                var allowedGrades = linkedTeacher.Relations
                    .Select(r => r.GradeId)
                    .Where(g => g != null)
                    .ToList();

                query = query.Where(s => allowedGrades.Contains(s.GradeId));
            }

            Students = await query.ToListAsync();

             // Server-Side Data Masking for non-Admin/non-Supervisor
            if (!User.IsInRole("Admin") && !User.IsInRole("Supervisor"))
            {
                foreach (var student in Students)
                {
                    student.StdMobile = null;
                    student.Mobile2 = null;
                    student.StdNationality = null; // Assuming string?
                    student.Note = null;
                }
            }
            
            ViewData["GradeList"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.GradeTables, "GradeId", "GradeName");
        }
        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var student = await _context.StdTables.FindAsync(id);

            if (student != null)
            {
                try
                {
                    _context.StdTables.Remove(student);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Student deleted successfully.";
                }
                catch (DbUpdateException)
                {
                    // Likely a foreign key constraint violation
                    TempData["ErrorMessage"] = "Cannot delete student. They may be linked to exams, grades, or other records.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Student not found.";
            }

            return RedirectToPage();
        }
    }
}