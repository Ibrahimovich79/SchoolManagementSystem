using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly SchoolDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(SchoolDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public int StartStudentCount { get; set; }
        public int TeacherCount { get; set; }
        public int ClassCount { get; set; }

        public bool IsTeacher { get; set; }
        public List<ClassViewModel> TeacherClasses { get; set; } = new();

        public class ClassViewModel
        {
            public string GradeId { get; set; }
            public string GradeName { get; set; }
            public int StudentCount { get; set; }
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var roles = await _userManager.GetRolesAsync(user);
            IsTeacher = roles.Contains("Teacher");

            if (IsTeacher)
            {
                // Get the linked TeacherTb record
                var teacher = await _context.TeacherTbs
                    .Include(t => t.Relations)
                    .ThenInclude(r => r.GradeNavigation)
                    .FirstOrDefaultAsync(t => t.UserId == user.Id);

                if (teacher != null)
                {
                    // Get classes assigned via Relation table
                    var assignedGrades = teacher.Relations
                        .Where(r => r.GradeId != null)
                        .Select(r => r.GradeNavigation)
                        .Distinct()
                        .OrderBy(g => g.GradeName)
                        .ToList();

                    foreach (var grade in assignedGrades)
                    {
                        var studentCount = await _context.StdTables
                            .CountAsync(s => s.GradeId == grade.GradeId);

                        TeacherClasses.Add(new ClassViewModel
                        {
                            GradeId = grade.GradeId,
                            GradeName = grade.GradeName ?? grade.GradeId,
                            StudentCount = studentCount
                        });
                    }
                }
            }
            else
            {
                // Admin / Supervisor View
                StartStudentCount = await _context.StdTables.CountAsync();
                TeacherCount = await _context.TeacherTbs.CountAsync();
                // Count unique ClassRooms (handling nulls if any)
                ClassCount = await _context.StdTables
                                           .Where(s => s.GradeId != null)
                                           .Select(s => s.GradeId)
                                           .Distinct()
                                           .CountAsync();
            }
        }
    }
}
