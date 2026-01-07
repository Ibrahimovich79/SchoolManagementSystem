using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

[Table("TeacherTb")]
[Index("CousreId", Name = "TeacherTb$CousreID")]
[Index("StaffId", Name = "TeacherTb$StaffID")]
public partial class TeacherTb
{
    [Key]
    [Column("TeachID")]
    public long TeachId { get; set; }

    [StringLength(255)]
    public string? TeachName { get; set; }

    [StringLength(450)]
    public string? UserId { get; set; }

    public int? TeachMobile { get; set; }

    [Column("StaffID")]
    public int? StaffId { get; set; }

    [Column("CousreID")]
    public int? CousreId { get; set; }

    [ForeignKey("CousreId")]
    [InverseProperty("TeacherTbs")]
    public virtual Course? Cousre { get; set; }

    [InverseProperty("Teach")]
    public virtual ICollection<Relation> Relations { get; set; } = new List<Relation>();
}
