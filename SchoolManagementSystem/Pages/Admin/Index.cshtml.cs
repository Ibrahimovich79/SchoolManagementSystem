using Microsoft.AspNetCore.Authorization; // Add this
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Pages.Admin
{
    // Ensure only Admins can access (Double verification on top of Program.cs convention)
    [Authorize(Roles = "Admin,Supervisor")]
    public class IndexModel : PageModel
    {
        private readonly SchoolManagementSystem.Data.SchoolDbContext _context;

        public IndexModel(SchoolManagementSystem.Data.SchoolDbContext context)
        {
            _context = context;
        }

        public int StartStudentCount { get; set; }
        public int TeacherCount { get; set; }
        public int ClassCount { get; set; }

        public async Task OnGetAsync()
        {
            StartStudentCount = await _context.StdTables.CountAsync();
            TeacherCount = await _context.TeacherTbs.CountAsync();
            ClassCount = await _context.GradeTables.CountAsync();
        }

        public async Task<IActionResult> OnPostSendReportAsync([FromServices] SchoolManagementSystem.Services.IAttendanceReportService reportService)
        {
            try
            {
                await reportService.SendDailyReportAsync();
                TempData["ReportMessage"] = "تم إرسال تقرير الغياب بنجاح إلى الإدارة.";
            }
            catch (Exception ex)
            {
                TempData["ReportError"] = "حدث خطأ أثناء إرسال التقرير: " + ex.Message;
            }
            return RedirectToPage();
        }
    }
}