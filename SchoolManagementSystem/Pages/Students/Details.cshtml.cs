using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Pages.Students
{
    public class DetailsModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public DetailsModel(SchoolDbContext context)
        {
            _context = context;
        }

        public StdTable Student { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Student = await _context.StdTables
                .Include(s => s.BusNoNavigation)
                .Include(s => s.StdGradeNavigation)
                .FirstOrDefaultAsync(m => m.Qid == id);

            if (Student == null)
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

            var studentToUpdate = await _context.StdTables.FindAsync(id);
            if (studentToUpdate == null)
            {
                return new JsonResult(new { success = false, message = "Student not found" });
            }

            string updatedValue = fieldValue;

            try
            {
                switch (fieldName)
                {
                    case "StdName":
                        studentToUpdate.StdName = fieldValue;
                        break;
                    case "StdGrade":
                        studentToUpdate.StdGrade = fieldValue;
                        break;
                    case "ClassRoom":
                        studentToUpdate.ClassRoom = fieldValue;
                        break;
                    case "StdMobile":
                        if (int.TryParse(fieldValue, out int mobileValue))
                        {
                            studentToUpdate.StdMobile = mobileValue;
                        }
                        break;
                    case "Mobile2":
                        if (double.TryParse(fieldValue, out double mobile2Value))
                        {
                            studentToUpdate.Mobile2 = mobile2Value;
                        }
                        break;
                    case "StdNationality":
                        studentToUpdate.StdNationality = fieldValue;
                        break;
                    case "Transport":
                        studentToUpdate.Transport = fieldValue;
                        break;
                    case "BusNo":
                        if (int.TryParse(fieldValue, out int busNoValue))
                        {
                            studentToUpdate.BusNo = busNoValue;
                        }
                        break;
                    case "Note":
                        studentToUpdate.Note = fieldValue;
                        break;
                    case "OldBranche":
                        studentToUpdate.OldBranche = fieldValue;
                        break;
                    case "OldSchool":
                        studentToUpdate.OldSchool = fieldValue;
                        break;
                    case "UserName":
                        studentToUpdate.UserName = fieldValue;
                        break;
                    case "StdSeat":
                        if (int.TryParse(fieldValue, out int seatValue))
                        {
                            studentToUpdate.StdSeat = seatValue;
                        }
                        break;
                    case "StdCode":
                        if (int.TryParse(fieldValue, out int codeValue))
                        {
                            studentToUpdate.StdCode = codeValue;
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
