using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Pages.Admin.Attendance
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class DailySummaryModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public DailySummaryModel(SchoolDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime ReportDate { get; set; } = DateTime.Today;

        public List<ClassSummaryViewModel> Summaries { get; set; } = new();
        public OverallSummaryViewModel OverallSummary { get; set; } = new();

        public class ClassSummaryViewModel
        {
            public string GradeId { get; set; }
            public string GradeName { get; set; }
            public int TotalStudents { get; set; }
            public int PresentCount { get; set; }
            public int AbsentCount { get; set; }
            public double AttendancePercentage { get; set; }
        }

        public class OverallSummaryViewModel
        {
            public int TotalStudents { get; set; }
            public int TotalPresent { get; set; }
            public int TotalAbsent { get; set; }
            public double TotalPercentage { get; set; }
        }

        public async Task OnGetAsync()
        {
            if (ReportDate == default) ReportDate = DateTime.Today;

            // Get Grades and Students (Exclude Home-Schooled)
            var grades = await _context.GradeTables
                .Where(g => !g.GradeName.Contains("المنازل") && !g.GradeName.Contains("Home"))
                .Include(g => g.StdTables)
                .ToListAsync();

            // Get Absences for date
            var absences = await _context.StudentAttendances
                .Where(a => a.AttendanceDate.Date == ReportDate.Date)
                .Select(a => a.StudentId)
                .ToListAsync();
            var absentSet = new HashSet<long>(absences);

            Summaries = grades.Select(g => 
            {
                var total = g.StdTables.Count;
                var absent = g.StdTables.Count(s => absentSet.Contains(s.Qid));
                var present = total - absent;

                return new ClassSummaryViewModel
                {
                    GradeId = g.GradeId,
                    GradeName = g.GradeName ?? g.GradeId,
                    TotalStudents = total,
                    PresentCount = present,
                    AbsentCount = absent,
                    AttendancePercentage = total > 0 ? Math.Round((double)present / total * 100, 1) : 0
                };
            }).OrderBy(s => s.GradeName).ToList();

            OverallSummary = new OverallSummaryViewModel
            {
                TotalStudents = Summaries.Sum(s => s.TotalStudents),
                TotalPresent = Summaries.Sum(s => s.PresentCount),
                TotalAbsent = Summaries.Sum(s => s.AbsentCount),
            };
            OverallSummary.TotalPercentage = OverallSummary.TotalStudents > 0 
                ? Math.Round((double)OverallSummary.TotalPresent / OverallSummary.TotalStudents * 100, 1) 
                : 0;
        }

        public async Task<IActionResult> OnPostExportAsync(DateTime reportDate)
        {
            var date = reportDate == default ? DateTime.Today : reportDate;
            
            // Get detailed absent list (Exclude Home-Schooled)
            var absences = await _context.StudentAttendances
                .Include(a => a.Student)
                .Include(a => a.Class)
                .Where(a => a.AttendanceDate.Date == date.Date && 
                            !a.Class.GradeName.Contains("المنازل") && 
                            !a.Class.GradeName.Contains("Home"))
                .Select(a => new
                {
                    Section = a.Class.GradeName ?? a.ClassId,
                    StudentName = a.Student.StdName,
                    Notes = a.Note ?? ""
                })
                .OrderBy(a => a.Section)
                .ThenBy(a => a.StudentName)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Section Name,Student Name,Notes");

            foreach (var item in absences)
            {
                var section = EscapeCsv(item.Section);
                var name = EscapeCsv(item.StudentName);
                var note = EscapeCsv(item.Notes);
                csv.AppendLine($"{section},{name},{note}");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"Attendance_Report_{date:yyyy-MM-dd}.csv");
        }

        private string EscapeCsv(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }
    }
}
