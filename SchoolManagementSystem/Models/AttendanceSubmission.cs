using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Models
{
    public class AttendanceSubmission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string ClassId { get; set; } = default!;

        [Required]
        [Column(TypeName = "date")]
        public DateTime AttendanceDate { get; set; }

        [Required]
        public long TeacherId { get; set; }

        public DateTime SubmittedAt { get; set; }

        // Navigation properties
        [ForeignKey("ClassId")]
        public virtual GradeTable ClassNavigation { get; set; } = default!;

        [ForeignKey("TeacherId")]
        public virtual TeacherTb TeacherNavigation { get; set; } = default!;
    }
}
