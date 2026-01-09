using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using System;
using System.Collections.Generic;
using System.IO;
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

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; } // "Present", "Absent", or null for All

        [BindProperty(SupportsGet = true)]
        public string? ClassFilter { get; set; } // GradeId or null for All

        public List<DailyReportItem> ReportData { get; set; } = new();
        public SelectList ClassList { get; set; }

        public class DailyReportItem
        {
            public long StudentId { get; set; }
            public string StudentName { get; set; }
            public string ClassName { get; set; }
            public string ClassId { get; set; }
            public string Status { get; set; }
            public string StudentNote { get; set; } // From StdTable.Note
            public string AttendanceNote { get; set; } // From StudentAttendance.Note
            public string SMS { get; set; } // From StdTable.Sms or StdMobile
            public string Mobile { get; set; } // From StdTable.StdMobile
            public string LastActionBy { get; set; } // Teacher/Supervisor name + time
        }

        public async Task OnGetAsync()
        {
            await LoadReportDataAsync();
        }

        private async Task LoadReportDataAsync()
        {
            if (ReportDate == default) ReportDate = DateTime.Today;

            // Load class list for dropdown (Exclude Home-Schooled)
            ClassList = new SelectList(await _context.GradeTables
                .Where(g => !g.GradeName.Contains("المنازل") && !g.GradeName.Contains("Home"))
                .OrderBy(g => g.GradeName).ToListAsync(), "GradeId", "GradeName");

            // 1. Get all students with their class info (Exclude Home-Schooled)
            var studentsQuery = _context.StdTables
                .Include(s => s.StdGradeNavigation)
                .Where(s => !s.StdGradeNavigation.GradeName.Contains("المنازل") && !s.StdGradeNavigation.GradeName.Contains("Home"))
                .AsQueryable();

            // Apply class filter
            if (!string.IsNullOrEmpty(ClassFilter))
            {
                studentsQuery = studentsQuery.Where(s => s.GradeId == ClassFilter);
            }

            var students = await studentsQuery
                .Select(s => new
                {
                    s.Qid,
                    s.StdName,
                    s.GradeId,
                    ClassName = s.StdGradeNavigation.GradeName ?? s.GradeId,
                    StudentNote = s.Note, // Important: Student's note from StdTable
                    SMS = s.Sms,
                    Mobile = s.StdMobile
                })
                .ToListAsync();

            // 2. Get attendance records for the selected date
            var attendanceRecords = await _context.StudentAttendances
                .Include(a => a.Teacher)
                .Where(a => a.AttendanceDate.Date == ReportDate.Date)
                .ToDictionaryAsync(a => a.StudentId);

            // 3. Merge data
            var reportData = students.OrderBy(s => s.ClassName).ThenBy(s => s.StdName).Select(s => 
            {
                var isAbsent = attendanceRecords.ContainsKey(s.Qid);
                var attRecord = isAbsent ? attendanceRecords[s.Qid] : null;

                return new DailyReportItem
                {
                    StudentId = s.Qid,
                    StudentName = s.StdName,
                    ClassId = s.GradeId,
                    ClassName = s.ClassName,
                    Status = isAbsent ? "Absent" : "Present",
                    StudentNote = s.StudentNote, // Student's permanent note
                    AttendanceNote = attRecord?.Note, // Attendance-specific note
                    SMS = s.SMS?.ToString(),
                    Mobile = s.Mobile?.ToString(),
                    LastActionBy = attRecord != null ? $"{attRecord.Teacher?.TeachName ?? "N/A"} ({attRecord.CreatedAt.ToLocalTime():hh:mm tt})" : "N/A"
                };
            });

            // Apply status filter
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                reportData = reportData.Where(r => r.Status == StatusFilter);
            }

            ReportData = reportData.ToList();
        }

        public async Task<IActionResult> OnPostExportAsync(DateTime reportDate, string? statusFilter, string? classFilter)
        {
            ReportDate = reportDate == default ? DateTime.Today : reportDate;
            StatusFilter = statusFilter;
            ClassFilter = classFilter;

            await LoadReportDataAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Attendance Report");

            // Header row
            worksheet.Cell(1, 1).Value = "Class";
            worksheet.Cell(1, 2).Value = "Student Name";
            worksheet.Cell(1, 3).Value = "Student ID";
            worksheet.Cell(1, 4).Value = "Status";
            worksheet.Cell(1, 5).Value = "SMS";
            worksheet.Cell(1, 6).Value = "Mobile";
            worksheet.Cell(1, 7).Value = "Student Note";
            worksheet.Cell(1, 8).Value = "Attendance Note";
            worksheet.Cell(1, 9).Value = "Last Action By / Supervisor's Name";

            // Style header
            var headerRange = worksheet.Range(1, 1, 1, 9);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

            // Data rows
            int row = 2;
            foreach (var item in ReportData)
            {
                worksheet.Cell(row, 1).Value = item.ClassName;
                worksheet.Cell(row, 2).Value = item.StudentName;
                worksheet.Cell(row, 3).Value = item.StudentId;
                worksheet.Cell(row, 4).Value = item.Status;
                worksheet.Cell(row, 5).Value = item.SMS;
                worksheet.Cell(row, 6).Value = item.Mobile;
                worksheet.Cell(row, 7).Value = item.StudentNote;
                worksheet.Cell(row, 8).Value = item.AttendanceNote;
                worksheet.Cell(row, 9).Value = item.LastActionBy;

                // Highlight absent students
                if (item.Status == "Absent")
                {
                    worksheet.Range(row, 1, row, 9).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightPink;
                }
                row++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Generate file
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Attendance_Report_{ReportDate:yyyy-MM-dd}";
            if (!string.IsNullOrEmpty(StatusFilter)) fileName += $"_{StatusFilter}";
            if (!string.IsNullOrEmpty(ClassFilter)) fileName += $"_{ClassFilter}";
            fileName += ".xlsx";

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
