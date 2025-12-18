using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

[Index("CourseId", Name = "ExamAbs$CourseID")]
[Index("StdId", Name = "ExamAbs$StdID")]
public partial class ExamAb
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("StdID")]
    public long? StdId { get; set; }

    [Column("CourseID")]
    public int? CourseId { get; set; }

    [StringLength(255)]
    public string? Stdstatus { get; set; }

    public bool? Present { get; set; }

    [Column("SSMA_TimeStamp")]
    public byte[] SsmaTimeStamp { get; set; } = null!;

    [ForeignKey("CourseId")]
    [InverseProperty("ExamAbs")]
    public virtual Course? Course { get; set; }

    [ForeignKey("StdId")]
    [InverseProperty("ExamAbs")]
    public virtual StdTable? Std { get; set; }
}
