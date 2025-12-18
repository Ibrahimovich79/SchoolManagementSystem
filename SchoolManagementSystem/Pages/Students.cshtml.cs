using Microsoft.AspNetCore.Authorization;
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

        public IList<StdTable> Students { get; set; } = default!;

        public async Task OnGetAsync(string sortOrder)
        {
            CurrentSort = sortOrder;
            IdSort = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            NameSort = sortOrder == "Name" ? "name_desc" : "Name";
            GradeSort = sortOrder == "Grade" ? "grade_desc" : "Grade";
            ClassSort = sortOrder == "Class" ? "class_desc" : "Class";

            IQueryable<StdTable> query = _context.StdTables;

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
                    query = query.OrderBy(s => s.StdGrade);
                    break;
                case "grade_desc":
                    query = query.OrderByDescending(s => s.StdGrade);
                    break;
                case "Class":
                    query = query.OrderBy(s => s.ClassRoom);
                    break;
                case "class_desc":
                    query = query.OrderByDescending(s => s.ClassRoom);
                    break;
                default:
                    query = query.OrderBy(s => s.Qid);
                    break;
            }

            Students = await query.ToListAsync();
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