using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Pages.Teachers
{
    public class DetailsModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public DetailsModel(SchoolDbContext context)
        {
            _context = context;
        }

        public TeacherTb Teacher { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Teacher = await _context.TeacherTbs
                .Include(t => t.Cousre)
                .Include(t => t.Relations)
                .ThenInclude(r => r.GradeNavigation)
                .FirstOrDefaultAsync(m => m.TeachId == id);

            if (Teacher == null)
            {
                return NotFound();
            }

            // Populate Grade Dropdown for assignment (exclude already assigned grades if possible, or just show all)
            // Ideally exclude ones the teacher already has
            var assignedGradeIds = Teacher.Relations.Where(r => r.GradeId != null).Select(r => r.GradeId).ToList();
            var availableGrades = await _context.GradeTables.Where(g => !assignedGradeIds.Contains(g.GradeId)).ToListAsync();
            
            ViewData["GradeId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(availableGrades, "GradeId", "GradeName");
            ViewData["CousreId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Courses.ToListAsync(), "CourseId", "CourseName");

            return Page();
        }

        public async Task<IActionResult> OnPostAddGradeAsync(long id, string gradeId)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Supervisor"))
            {
                 return Forbid();
            }
            
            // Check if already exists
            var exists = await _context.Relations.AnyAsync(r => r.TeachId == id && r.GradeId == gradeId);
            if (!exists)
            {
                var relation = new Relation
                {
                    TeachId = id,
                    GradeId = gradeId, 
                };
                _context.Relations.Add(relation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Grade assigned successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Grade already assigned.";
            }
            
            return RedirectToPage(new { id = id });
        }

         public async Task<IActionResult> OnPostRemoveGradeAsync(long teachId, string gradeId)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Supervisor"))
            {
                 return Forbid();
            }

            var relation = await _context.Relations.FirstOrDefaultAsync(r => r.TeachId == teachId && r.GradeId == gradeId);
            if (relation != null)
            {
                _context.Relations.Remove(relation);
                await _context.SaveChangesAsync();
                 TempData["SuccessMessage"] = "Grade removed successfully.";
            }
             else
            {
                TempData["ErrorMessage"] = "Assignment not found.";
            }

            return RedirectToPage(new { id = teachId });
        }

        public async Task<IActionResult> OnPostUpdateFieldAsync(long id, string fieldName, string fieldValue)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Supervisor"))
            {
                return new JsonResult(new { success = false, message = "Unauthorized" });
            }

            var teacherToUpdate = await _context.TeacherTbs.FindAsync(id);
            if (teacherToUpdate == null)
            {
                return new JsonResult(new { success = false, message = "Teacher not found" });
            }

            string updatedValue = fieldValue;

            try
            {
                switch (fieldName)
                {
                    case "TeachName":
                        teacherToUpdate.TeachName = fieldValue;
                        break;
                    case "TeachId":
                        // ... logic existing ...
                        if (long.TryParse(fieldValue, out long newId))
                        {
                            if (newId != id)
                            {
                                if (await _context.TeacherTbs.AnyAsync(t => t.TeachId == newId))
                                {
                                    return new JsonResult(new { success = false, message = "Teacher ID already exists." });
                                }
                                
                                try 
                                {
                                    await _context.Database.ExecuteSqlRawAsync("UPDATE TeacherTb SET TeachID = {0} WHERE TeachID = {1}", newId, id);
                                    return new JsonResult(new { success = true, shouldReload = true, redirectUrl = "/Teachers/Details/" + newId });
                                }
                                catch (Exception ex)
                                {
                                     return new JsonResult(new { success = false, message = "Could not update ID. Error: " + ex.Message });
                                }
                            }
                        }
                        break;
                    case "TeachMobile":
                        if (int.TryParse(fieldValue, out int mobileValue))
                        {
                            teacherToUpdate.TeachMobile = mobileValue;
                        }
                        break;
                    case "StaffId":
                        if (int.TryParse(fieldValue, out int staffIdValue))
                        {
                            teacherToUpdate.StaffId = staffIdValue;
                        }
                        break;
                    case "CousreId":
                        if (int.TryParse(fieldValue, out int courseIdValue))
                        {
                            teacherToUpdate.CousreId = courseIdValue;
                            var course = await _context.Courses.FindAsync(courseIdValue);
                            updatedValue = course?.CourseName ?? fieldValue;
                        }
                        break;
                     default:
                         return new JsonResult(new { success = false, message = "Invalid field name" });
                }

                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true, newValue = updatedValue });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}
