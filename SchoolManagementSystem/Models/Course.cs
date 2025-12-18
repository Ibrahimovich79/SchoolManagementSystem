using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

public partial class Course
{
    [Key]
    [Column("CourseID")]
    public int CourseId { get; set; }

    [StringLength(255)]
    public string? CourseName { get; set; }

    [Column("courseDateN")]
    [Precision(0)]
    public DateTime? CourseDateN { get; set; }

    [StringLength(255)]
    public string? Note { get; set; }

    [InverseProperty("Cousre")]
    public virtual ICollection<CousreDate> CousreDates { get; set; } = new List<CousreDate>();

    [InverseProperty("Course")]
    public virtual ICollection<ExamAb> ExamAbs { get; set; } = new List<ExamAb>();

    [InverseProperty("Cousre")]
    public virtual ICollection<TeacherTb> TeacherTbs { get; set; } = new List<TeacherTb>();
}
