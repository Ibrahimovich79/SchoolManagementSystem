using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SchoolDbContext _context;

        public EditModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SchoolDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string UserId { get; set; } = string.Empty;

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            public List<string> SelectedRoles { get; set; } = new();

            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New Password (Optional)")]
            public string? NewPassword { get; set; }

            [Display(Name = "Teacher")]
            public long? TeacherId { get; set; }

            [Display(Name = "Student")]
            public long? StudentId { get; set; }
        }

        public List<string> AllRoles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            UserId = user.Id;
            Input.Email = user.Email ?? string.Empty;

            // Get all roles
            AllRoles = _roleManager.Roles.Select(r => r.Name!).ToList();

            // Get user roles
            var userRoles = await _userManager.GetRolesAsync(user);
            Input.SelectedRoles = userRoles.ToList();

            await PopulateDropdowns(user.Id);

            // Set current selections
            // Check if user is linked to a Teacher
            var linkedTeacher = await _context.TeacherTbs.FirstOrDefaultAsync(t => t.UserId == user.Id);
            if (linkedTeacher != null)
            {
                Input.TeacherId = linkedTeacher.TeachId;
            }

            // Check if user is linked to a Student
            var linkedStudent = await _context.StdTables.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (linkedStudent != null)
            {
                Input.StudentId = linkedStudent.Qid;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            UserId = user.Id;
            AllRoles = _roleManager.Roles.Select(r => r.Name!).ToList();

            if (!ModelState.IsValid)
            {
                 await PopulateDropdowns(user.Id);
                 return Page();
            }

            // Update Email/Username
            if (Input.Email != user.Email)
            {
                user.Email = Input.Email;
                user.UserName = Input.Email; 
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    await PopulateDropdowns(user.Id);
                    return Page();
                }
            }

            // Password Reset Logic
            if (!string.IsNullOrEmpty(Input.NewPassword))
            {
                var removeResult = await _userManager.RemovePasswordAsync(user);
                if (removeResult.Succeeded)
                {
                    var addResult = await _userManager.AddPasswordAsync(user, Input.NewPassword);
                    if (!addResult.Succeeded)
                    {
                         foreach (var error in addResult.Errors)
                         {
                             ModelState.AddModelError(string.Empty, "Password Error: " + error.Description);
                         }
                         await PopulateDropdowns(user.Id);
                         return Page();
                    }
                }
                else
                {
                     foreach (var error in removeResult.Errors)
                     {
                         ModelState.AddModelError(string.Empty, "Error removing old password: " + error.Description);
                     }
                     await PopulateDropdowns(user.Id);
                     return Page();
                }
            }

            // Update Roles
            var userRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = Input.SelectedRoles ?? new List<string>();

            var rolesToAdd = selectedRoles.Except(userRoles);
            var rolesToRemove = userRoles.Except(selectedRoles);

            await _userManager.AddToRolesAsync(user, rolesToAdd);
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            // Handle Linking Logic
            bool isTeacherOrSupervisor = selectedRoles.Contains("Teacher") || selectedRoles.Contains("Supervisor");
            bool isStudent = selectedRoles.Contains("Student");

            // 1. Handle Teacher Link
            // First unlink any teacher currently linked to this user (to handle switches or removals)
            var currentLinkedTeacher = await _context.TeacherTbs.FirstOrDefaultAsync(t => t.UserId == user.Id);
            if (currentLinkedTeacher != null)
            {
                // If not role teacher/supervisor OR selected teacher is different OR no teacher selected
                if (!isTeacherOrSupervisor || Input.TeacherId != currentLinkedTeacher.TeachId || Input.TeacherId == null)
                {
                    currentLinkedTeacher.UserId = null;
                    _context.TeacherTbs.Update(currentLinkedTeacher);
                }
            }

            // Then link the new teacher if applicable
            if (isTeacherOrSupervisor && Input.TeacherId.HasValue)
            {
                // Find the teacher to link
                var newTeacher = await _context.TeacherTbs.FindAsync(Input.TeacherId.Value);
                if (newTeacher != null && newTeacher.UserId != user.Id) // Only update if not already linked to this user
                {
                    // If this teacher was linked to someone else, unlink them? (Optional, but assumed safe based on 'unlinked only in dropdown')
                    // Actually, the dropdown only shows unlinked ones + current user's one.
                    
                    newTeacher.UserId = user.Id;
                    _context.TeacherTbs.Update(newTeacher);
                }
            }

            // 2. Handle Student Link
            // First unlink any student currently linked
            var currentLinkedStudent = await _context.StdTables.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (currentLinkedStudent != null)
            {
                if (!isStudent || Input.StudentId != currentLinkedStudent.Qid || Input.StudentId == null)
                {
                    currentLinkedStudent.UserId = null;
                    _context.StdTables.Update(currentLinkedStudent);
                }
            }

            // Then link new student
            if (isStudent && Input.StudentId.HasValue)
            {
                var newStudent = await _context.StdTables.FindAsync(Input.StudentId.Value);
                if (newStudent != null && newStudent.UserId != user.Id)
                {
                    newStudent.UserId = user.Id;
                    _context.StdTables.Update(newStudent);
                }
            }
            
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToPage("./Index");
        }

        private async Task PopulateDropdowns(string currentUserId)
        {
            // Unlinked Teachers + Currently Linked Teacher
            var teachersQuery = _context.TeacherTbs.Where(t => t.UserId == null || t.UserId == currentUserId);
            var teachersList = await teachersQuery.Select(t => new { t.TeachId, t.TeachName }).ToListAsync();
            ViewData["Teachers"] = new SelectList(teachersList, "TeachId", "TeachName");

            // Unlinked Students + Currently Linked Student
            var studentsQuery = _context.StdTables.Where(s => s.UserId == null || s.UserId == currentUserId);
            var studentsList = await studentsQuery.Select(s => new { QID = s.Qid, Name = s.StdName + " (Code: " + s.StdCode + ")" }).ToListAsync();
            ViewData["Students"] = new SelectList(studentsList, "QID", "Name");
        }
    }
}
