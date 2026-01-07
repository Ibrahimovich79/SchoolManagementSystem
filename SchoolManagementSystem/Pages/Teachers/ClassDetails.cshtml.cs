using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Pages.Teachers
{
    [Authorize(Roles = "Teacher,Admin,Supervisor")] // Ensure teachers can access
    public class ClassDetailsModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public ClassDetailsModel(SchoolDbContext context)
        {
            _context = context;
        }

        public string ClassName { get; set; }
        public List<StudentViewModel> Students { get; set; } = new();

        public class StudentViewModel
        {
            public long StudentId { get; set; } // Personal Number (QID)
            public string Name { get; set; }
            public int StudentCode { get; set; }
            public string TransportType { get; set; }
            public int? BusNo { get; set; } // Added to display the specific number
            public string Note { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string gradeId)
        {
            if (string.IsNullOrEmpty(gradeId))
            {
                return NotFound();
            }

            // Get Grade info
            var grade = await _context.GradeTables.FirstOrDefaultAsync(g => g.GradeId == gradeId);
            if (grade == null)
            {
                return NotFound();
            }
            ClassName = grade.GradeName ?? grade.GradeId; // Fallback if Name is null

            // Get Students in this grade
            var students = await _context.StdTables
                .Where(s => s.GradeId == gradeId)
                .Select(s => new
                {
                    s.Qid,
                    s.StdName,
                    s.StdCode,
                    s.BusNo,
                    s.Note
                })
                .ToListAsync();

            Students = students.Select(s => new StudentViewModel
            {
                StudentId = s.Qid,
                Name = s.StdName,
                StudentCode = s.StdCode,
                TransportType = (s.BusNo.HasValue && s.BusNo > 0) ? "Bus" : "Car",
                BusNo = s.BusNo,
                Note = s.Note
            }).ToList();

            return Page();
        }
    }
}
