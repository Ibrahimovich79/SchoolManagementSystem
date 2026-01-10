using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Pages.Admin.Buses
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class IndexModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public IndexModel(SchoolDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? BusFilter { get; set; }

        public IList<StdTable> Students { get; set; } = default!;
        public SelectList BusList { get; set; } = default!;
        
        // Supervisor Info for the selected bus
        public string BusSupervisor { get; set; } = string.Empty;
        public string BusSupervisorMobile { get; set; } = string.Empty;
        public string BusAddress { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var buses = await _context.BusTables
                .OrderBy(b => b.BusNo)
                .ToListAsync();
            
            BusList = new SelectList(buses, "BusNo", "BusNo");

            var query = _context.StdTables
                .Include(s => s.StdGradeNavigation)
                .Include(s => s.BusNoNavigation)
                .AsQueryable();

            if (BusFilter.HasValue)
            {
                if (BusFilter.Value == -1) // Special case for "Car Students"
                {
                    query = query.Where(s => s.BusNo == null);
                }
                else
                {
                    query = query.Where(s => s.BusNo == BusFilter.Value);
                    
                    var currentBus = buses.FirstOrDefault(b => b.BusNo == BusFilter.Value);
                    if (currentBus != null)
                    {
                        BusSupervisor = currentBus.BusSupervisor;
                        BusSupervisorMobile = currentBus.SupervisorMobile?.ToString() ?? string.Empty;
                        BusAddress = currentBus.Address;
                    }
                }
            }
            else
            {
                // All Buses: Exclude non-riders (NULL)
                query = query.Where(s => s.BusNo != null);
            }

            Students = await query
                .OrderBy(s => s.BusNo)
                .ThenBy(s => s.StdGradeNavigation.GradeName)
                .ThenBy(s => s.StdName)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostUpdateBusAsync(long studentId, int? newBusNo)
        {
            var student = await _context.StdTables.FindAsync(studentId);
            if (student == null)
            {
                return new JsonResult(new { success = false, message = "Student not found" });
            }

            // If newBusNo is 0 or null, set to null (Car Student)
            if (newBusNo == null || newBusNo <= 0)
            {
                student.BusNo = null;
            }
            else
            {
                // Verify bus exists? 
                // For simplicity, we just allow it if it's in the DB or we just trust the input if it's a number.
                // But let's be safe.
                var busExists = await _context.BusTables.AnyAsync(b => b.BusNo == newBusNo);
                if (newBusNo.HasValue && !busExists)
                {
                    // Optionally create the bus or return error. 
                    // Most systems would want to select from existing.
                }
                student.BusNo = newBusNo;
            }

            try
            {
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }
            catch (System.Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostExportAsync(int? busFilter)
        {
            BusFilter = busFilter;
            await LoadDataAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Bus List");
            worksheet.RightToLeft = true;

            string title = "كشف ركاب الحافلات";
            if (BusFilter.HasValue && BusFilter > 0) title = $"كشف ركاب الحافلة رقم {BusFilter}";
            else if (BusFilter == -1) title = "قائمة طلاب السيارات (بدون باص)";

            // Title and Info
            worksheet.Cell(1, 1).Value = "مدرسة معيذر الابتدائية للبنين";
            worksheet.Range(1, 1, 1, 14).Merge().Style.Font.Bold = true;
            worksheet.Range(1, 1, 1, 14).Style.Font.FontSize = 18;
            worksheet.Range(1, 1, 1, 14).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

            worksheet.Cell(2, 1).Value = title;
            worksheet.Range(2, 1, 2, 14).Merge().Style.Font.Bold = true;
            worksheet.Range(2, 1, 2, 14).Style.Font.FontSize = 14;
            worksheet.Range(2, 1, 2, 14).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

            if (BusFilter.HasValue && BusFilter > 0)
            {
                worksheet.Cell(3, 1).Value = $"المشرف: {BusSupervisor} | الجوال: {BusSupervisorMobile} | المنطقة: {BusAddress}";
                worksheet.Range(3, 1, 3, 14).Merge().Style.Font.Bold = true;
                worksheet.Range(3, 1, 3, 14).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            }

            // Headers
            int headerRow = 5;
            worksheet.Cell(headerRow, 1).Value = "م";
            worksheet.Cell(headerRow, 2).Value = "اسم الطالب";
            worksheet.Cell(headerRow, 3).Value = "الأحد (ص)";
            worksheet.Cell(headerRow, 4).Value = "الأحد (م)";
            worksheet.Cell(headerRow, 5).Value = "الاثنين (ص)";
            worksheet.Cell(headerRow, 6).Value = "الاثنين (م)";
            worksheet.Cell(headerRow, 7).Value = "الثلاثاء (ص)";
            worksheet.Cell(headerRow, 8).Value = "الثلاثاء (م)";
            worksheet.Cell(headerRow, 9).Value = "الأربعاء (ص)";
            worksheet.Cell(headerRow, 10).Value = "الأربعاء (م)";
            worksheet.Cell(headerRow, 11).Value = "الخميس (ص)";
            worksheet.Cell(headerRow, 12).Value = "الخميس (م)";
            worksheet.Cell(headerRow, 13).Value = "الصف";
            worksheet.Cell(headerRow, 14).Value = "الجوال";

            var headerRange = worksheet.Range(headerRow, 1, headerRow, 14);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightSkyBlue;
            headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

            int row = headerRow + 1;
            int counter = 1;
            foreach (var student in Students)
            {
                worksheet.Cell(row, 1).Value = counter++;
                worksheet.Cell(row, 2).Value = student.StdName;
                
                // Keep attendance columns empty for manual marking
                for (int c = 3; c <= 12; c++) worksheet.Cell(row, c).Value = "";

                worksheet.Cell(row, 13).Value = student.StdGradeNavigation?.GradeName;
                worksheet.Cell(row, 14).Value = student.StdMobile;
                
                worksheet.Range(row, 1, row, 14).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                worksheet.Range(row, 1, row, 14).Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new System.IO.MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"Bus_Attendance_{System.DateTime.Now:yyyyMMdd}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
