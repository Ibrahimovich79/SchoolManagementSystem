using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models
{
    [Table("StudentAttendance")]
    [Index(nameof(StudentId), nameof(AttendanceDate), IsUnique = true)]
    public class StudentAttendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public long StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual StdTable Student { get; set; }

        [Required]
        [StringLength(255)]
        public string ClassId { get; set; } // Matches GradeId type in GradeTable

        [ForeignKey("ClassId")]
        public virtual GradeTable Class { get; set; }

        [Required]
        public long TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public virtual TeacherTb Teacher { get; set; }

        [Required]
        public DateTime AttendanceDate { get; set; }

        public bool IsAbsent { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
