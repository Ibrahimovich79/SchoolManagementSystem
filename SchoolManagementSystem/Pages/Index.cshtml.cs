using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public IndexModel(SchoolDbContext context)
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
            // Count unique ClassRooms (handling nulls if any)
            ClassCount = await _context.StdTables
                                       .Where(s => s.ClassRoom != null)
                                       .Select(s => s.ClassRoom)
                                       .Distinct()
                                       .CountAsync();
        }
    }
}
