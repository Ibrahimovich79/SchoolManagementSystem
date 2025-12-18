using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

[Table("Relation")]
[Index("Id", Name = "Relation$ID")]
[Index("TeachId", Name = "Relation$TeachID")]
public partial class Relation
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(255)]
    public string Grade { get; set; } = null!;

    [Column("TeachID")]
    public long? TeachId { get; set; }

    [ForeignKey("Grade")]
    [InverseProperty("Relations")]
    public virtual GradeTable GradeNavigation { get; set; } = null!;

    [ForeignKey("TeachId")]
    [InverseProperty("Relations")]
    public virtual TeacherTb? Teach { get; set; }
}
