using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Pages.Teachers
{
    [Authorize(Roles = "Teacher,Admin,Supervisor")] // Ensure teachers can access
    public class ClassDetailsModel : PageModel
    {
        private readonly SchoolDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ClassDetailsModel(SchoolDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public string ClassName { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string GradeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime AttendanceDate { get; set; } = DateTime.Today;

        public List<StudentViewModel> Students { get; set; } = new();

        [BindProperty]
        public List<long> AbsentStudentIds { get; set; } = new();

        public class StudentViewModel
        {
            public long StudentId { get; set; } // Personal Number (QID)
            public string Name { get; set; }
            public int StudentCode { get; set; }
            public string TransportType { get; set; }
            public int? BusNo { get; set; }
            public string Note { get; set; }
            public bool IsAbsent { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(GradeId))
            {
                return NotFound();
            }

            // Enforce Today only for Teachers
            if (User.IsInRole("Teacher") && !User.IsInRole("Admin") && !User.IsInRole("Supervisor"))
            {
                AttendanceDate = DateTime.Today;
            }
            else
            {
                // Ensure valid date for others
                if (AttendanceDate == default) AttendanceDate = DateTime.Today;
            }

            // Get Grade info
            var grade = await _context.GradeTables.FirstOrDefaultAsync(g => g.GradeId == GradeId);
            if (grade == null)
            {
                return NotFound();
            }
            ClassName = grade.GradeName ?? grade.GradeId; 

            // Get Students in this grade
            var students = await _context.StdTables
                .Where(s => s.GradeId == GradeId)
                .Select(s => new
                {
                    s.Qid,
                    s.StdName,
                    s.StdCode,
                    s.BusNo,
                    s.Note
                })
                .ToListAsync();

            // Get Attendance Records for this Date and Class
            var absenceRecords = await _context.StudentAttendances
                .Where(a => a.ClassId == GradeId && a.AttendanceDate.Date == AttendanceDate.Date)
                .Select(a => a.StudentId)
                .ToListAsync();

            Students = students.Select(s => new StudentViewModel
            {
                StudentId = s.Qid, // Using Qid as StudentId based on existing code
                Name = s.StdName,
                StudentCode = s.StdCode,
                TransportType = (s.BusNo.HasValue && s.BusNo > 0) ? "Bus" : "Car",
                BusNo = s.BusNo,
                Note = s.Note,
                IsAbsent = absenceRecords.Contains(s.Qid)
            }).OrderBy(s => s.Name).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(GradeId)) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Enforce Today only for Teachers
            if (User.IsInRole("Teacher") && !User.IsInRole("Admin") && !User.IsInRole("Supervisor"))
            {
                if (AttendanceDate.Date != DateTime.Today)
                {
                    ModelState.AddModelError("", "يسمح للمعلم بتسجيل الحضور لليوم فقط.");
                    return await OnGetAsync();
                }
            }

            // Get Teacher ID
            var teacher = await _context.TeacherTbs.FirstOrDefaultAsync(t => t.UserId == user.Id);
            
            if (teacher == null)
            {
                if (User.IsInRole("Admin") || User.IsInRole("Supervisor"))
                {
                    // Create a placeholder teacher record for Admin/Supervisor if missing
                    teacher = new TeacherTb
                    {
                        UserId = user.Id,
                        TeachName = user.Email?.Split('@')[0] ?? "Administrator"
                    };
                    _context.TeacherTbs.Add(teacher);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    ModelState.AddModelError("", "لم يتم العثور على سجل للمعلم.");
                    return await OnGetAsync();
                }
            }
            
            long teacherId = teacher.TeachId;

            // Fetch existing attendance for this day/class
            // Note: We use unique constraint on StudentId+Date, but here we scope by ClassId too just in case 
            // the student moved classes. But the constraint is global.
            var existingRecords = await _context.StudentAttendances
                .Where(a => a.ClassId == GradeId && a.AttendanceDate.Date == AttendanceDate.Date)
                .ToListAsync();

            var existingAbsentIds = existingRecords.Select(a => a.StudentId).ToList();
            
            // Determine Inserts (In Posted list but not in DB)
            var toInsert = AbsentStudentIds.Except(existingAbsentIds).ToList();

            // Determine Deletes (In DB but not in Posted list)
            var toDelete = existingAbsentIds.Except(AbsentStudentIds).ToList();

            if (toInsert.Any())
            {
                var newRecords = toInsert.Select(sid => new StudentAttendance
                {
                    StudentId = sid,
                    ClassId = GradeId,
                    TeacherId = teacherId,
                    AttendanceDate = AttendanceDate.Date,
                    IsAbsent = true,
                    CreatedAt = DateTime.UtcNow
                });
                _context.StudentAttendances.AddRange(newRecords);
            }

            if (toDelete.Any())
            {
                var recordsToDelete = existingRecords.Where(r => toDelete.Contains(r.StudentId));
                _context.StudentAttendances.RemoveRange(recordsToDelete);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حفظ كشف الحضور بنجاح.";

            // Redirect to get logic to refresh clean state
            return RedirectToPage(new { gradeId = GradeId, attendanceDate = AttendanceDate.ToString("yyyy-MM-dd") });
        }
    }
}
