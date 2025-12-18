using Microsoft.AspNetCore.Authorization; // Add this
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SchoolManagementSystem.Pages.Admin
{
    // Ensure only Admins can access (Double verification on top of Program.cs convention)
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}