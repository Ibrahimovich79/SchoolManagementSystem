using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Pages.Admin.Attendance
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class DailyReportModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public DailyReportModel(SchoolDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime ReportDate { get; set; } = DateTime.Today;

        public List<DailyReportItem> ReportData { get; set; } = new();

        public class DailyReportItem
        {
            public long StudentId { get; set; }
            public string StudentName { get; set; }
            public string ClassName { get; set; }
            public string Status { get; set; }
            public string Note { get; set; } // Attendance note
        }

        public async Task OnGetAsync()
        {
            if (ReportDate == default) ReportDate = DateTime.Today;

            // 1. Get all students with their class info
            var students = await _context.StdTables
                .Include(s => s.StdGradeNavigation)
                .Select(s => new
                {
                    s.Qid, // StudentId
                    s.StdName,
                    ClassName = s.StdGradeNavigation.GradeName ?? s.GradeId
                })
                .ToListAsync();

            // 2. Get attendance records for the selected date
            var attendanceRecords = await _context.StudentAttendances
                .Where(a => a.AttendanceDate.Date == ReportDate.Date)
                .ToDictionaryAsync(a => a.StudentId);

            // 3. Merge data
            ReportData = students.OrderBy(s => s.ClassName).ThenBy(s => s.StdName).Select(s => 
            {
                var isAbsent = attendanceRecords.ContainsKey(s.Qid);
                var attRecord = isAbsent ? attendanceRecords[s.Qid] : null;

                return new DailyReportItem
                {
                    StudentId = s.Qid,
                    StudentName = s.StdName,
                    ClassName = s.ClassName,
                    Status = isAbsent ? "Absent" : "Present",
                    Note = attRecord?.Note
                };
            }).ToList();
        }
    }
}
