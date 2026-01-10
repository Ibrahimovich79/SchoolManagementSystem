using System.Threading.Tasks;

namespace SchoolManagementSystem.Services
{
    public interface IAttendanceReportService
    {
        Task SendDailyReportAsync();
    }
}
