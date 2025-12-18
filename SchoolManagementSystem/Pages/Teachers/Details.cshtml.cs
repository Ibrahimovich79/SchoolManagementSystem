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
                .FirstOrDefaultAsync(m => m.TeachId == id);

            if (Teacher == null)
            {
                return NotFound();
            }
            return Page();
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
                        if (long.TryParse(fieldValue, out long newId))
                        {
                            if (newId != id)
                            {
                                if (await _context.TeacherTbs.AnyAsync(t => t.TeachId == newId))
                                {
                                    return new JsonResult(new { success = false, message = "Teacher ID already exists." });
                                }
                                
                                // Attempt to update Primary Key via Raw SQL to bypass EF tracking restrictions
                                // Note: This may fail if Foreign Keys exist and Cascade Update is not enabled in DB
                                try 
                                {
                                    await _context.Database.ExecuteSqlRawAsync("UPDATE TeacherTb SET TeachID = {0} WHERE TeachID = {1}", newId, id);
                                    return new JsonResult(new { success = true, shouldReload = true, redirectUrl = "/Teachers/Details/" + newId });
                                }
                                catch (Exception ex)
                                {
                                     return new JsonResult(new { success = false, message = "Could not update ID. Ensure no related records block this change. Error: " + ex.Message });
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
