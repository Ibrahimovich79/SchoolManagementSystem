using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Pages.Admin.Attendance
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class IndexModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public IndexModel(SchoolDbContext context)
        {
            _context = context;
        }

        public List<GradeViewModel> Grades { get; set; } = new();

        public class GradeViewModel
        {
            public string GradeId { get; set; }
            public string GradeName { get; set; }
            public int StudentCount { get; set; }
        }

        public async Task OnGetAsync()
        {
            Grades = await _context.GradeTables
                .Select(g => new GradeViewModel
                {
                    GradeId = g.GradeId,
                    GradeName = g.GradeName ?? g.GradeId,
                    StudentCount = g.StdTables.Count()
                })
                .OrderBy(g => g.GradeName)
                .ToListAsync();
        }
    }
}
