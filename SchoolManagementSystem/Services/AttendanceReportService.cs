using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;
using System.Text;

namespace SchoolManagementSystem.Services
{
    public class AttendanceReportService : BackgroundService, IAttendanceReportService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AttendanceReportService> _logger;
        private readonly IConfiguration _configuration;

        public AttendanceReportService(
            IServiceScopeFactory scopeFactory,
            ILogger<AttendanceReportService> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 13, 30, 0);

                if (now > scheduledTime)
                {
                    scheduledTime = scheduledTime.AddDays(1);
                }

                var delay = scheduledTime - now;
                _logger.LogInformation("AttendanceReportService scheduled to run in {Delay}", delay);

                await Task.Delay(delay, stoppingToken);

                try
                {
                    await SendDailyReportAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending daily attendance report.");
                }
            }
        }

        public async Task SendDailyReportAsync()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                var adminEmail = _configuration["Smtp:AdminEmail"] ?? "aismartchoices961@gmail.com";

                var today = DateTime.Today;

                // 1. Get Today's Absences
                var absencesToday = await context.StudentAttendances
                    .Include(a => a.Student)
                    .Include(a => a.Class)
                    .Where(a => a.AttendanceDate.Date == today)
                    .ToListAsync();

                // 2. Get Submission Status
                var allGradeIds = await context.GradeTables.Select(g => g.GradeId).ToListAsync();
                var submissionIdsToday = await context.AttendanceSubmissions
                    .Where(s => s.AttendanceDate.Date == today)
                    .Select(s => s.ClassId)
                    .ToListAsync();

                var pendingClasses = await context.GradeTables
                    .Where(g => !submissionIdsToday.Contains(g.GradeId))
                    .OrderBy(g => g.GradeName)
                    .ToListAsync();

                var completionPercentage = allGradeIds.Count > 0 
                    ? (double)submissionIdsToday.Count / allGradeIds.Count * 100 
                    : 0;

                // 3. Generate HTML
                var html = GenerateReportHtml(today, absencesToday, pendingClasses, completionPercentage);

                // 4. Send Email
                var subject = $"📊 تقرير الغياب اليومي - مدرسة معيذر الابتدائية - {today:yyyy-MM-dd}";
                await emailSender.SendEmailAsync(adminEmail, subject, html);
                _logger.LogInformation("Daily attendance report sent to {AdminEmail}", adminEmail);
            }
        }

        private string GenerateReportHtml(DateTime date, List<StudentAttendance> absences, List<GradeTable> pendingClasses, double completionStatus)
        {
            var sb = new StringBuilder();
            sb.Append("<div dir='rtl' style='font-family: Arial, sans-serif; color: #333;'>");
            sb.Append("<h2 style='color: #0369a1; border-bottom: 2px solid #e0f2fe; padding-bottom: 10px;'>📊 تقرير الغياب اليومي</h2>");
            sb.Append($"<p><strong>التاريخ:</strong> {date:yyyy-MM-dd}</p>");

            // Summary Section
            sb.Append("<div style='background-color: #f0f9ff; padding: 15px; border-radius: 8px; margin-bottom: 20px;'>");
            sb.Append("<h3 style='margin-top: 0; color: #0284c7;'>ملخص اليوم</h3>");
            sb.Append($"<p>⚠️ <strong>إجمالي الغائبين:</strong> {absences.Count} طالباً</p>");
            sb.Append($"<p>✅ <strong>نسبة اكتمال الإدخال:</strong> {completionStatus:F1}%</p>");
            sb.Append("</div>");

            // Alert Section (Pending Classes)
            if (pendingClasses.Any())
            {
                sb.Append("<div style='background-color: #fef2f2; padding: 15px; border-radius: 8px; border: 1px solid #fee2e2; margin-bottom: 20px;'>");
                sb.Append("<h3 style='margin-top: 0; color: #b91c1c;'>صفوف لم يتم رصد حضورها</h3>");
                sb.Append("<ul style='padding-right: 20px;'>");
                foreach (var grade in pendingClasses)
                {
                    sb.Append($"<li style='color: #991b1b;'>{grade.GradeName}</li>");
                }
                sb.Append("</ul>");
                sb.Append("</div>");
            }
            else
            {
                sb.Append("<p style='color: #15803d;'>✅ تم رصد الحضور لجميع الصفوف.</p>");
            }

            // Detailed Table
            if (absences.Any())
            {
                sb.Append("<h3 style='color: #0369a1;'>قائمة الطلاب الغائبين</h3>");
                sb.Append("<table style='width: 100%; border-collapse: collapse; margin-top: 10px;'>");
                sb.Append("<thead><tr style='background-color: #e0f2fe; text-align: right;'>");
                sb.Append("<th style='padding: 10px; border: 1px solid #cbd5e1;'>اسم الطالب</th>");
                sb.Append("<th style='padding: 10px; border: 1px solid #cbd5e1;'>الصف</th>");
                sb.Append("<th style='padding: 10px; border: 1px solid #cbd5e1;'>المواصلات</th>");
                sb.Append("</tr></thead>");
                sb.Append("<tbody>");

                foreach (var abs in absences)
                {
                    var transport = abs.Student?.BusNo > 0 ? "حافلة" : "سيارة";
                    sb.Append("<tr>");
                    sb.Append($"<td style='padding: 10px; border: 1px solid #cbd5e1;'>{abs.Student?.StdName}</td>");
                    sb.Append($"<td style='padding: 10px; border: 1px solid #cbd5e1;'>{abs.Class?.GradeName}</td>");
                    sb.Append($"<td style='padding: 10px; border: 1px solid #cbd5e1;'>{transport}</td>");
                    sb.Append("</tr>");
                }

                sb.Append("</tbody></table>");
            }

            sb.Append("</div>");
            return sb.ToString();
        }
    }
}
