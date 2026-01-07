using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

[Table("StdTable")]
[Index("StdCode", Name = "StdTable$StdCode", IsUnique = true)]
public partial class StdTable
{
    [Key]
    [Column("QID")]
    public long Qid { get; set; }

    [StringLength(255)]
    public string? StdName { get; set; }

    [StringLength(255)]
    public string? OldBranche { get; set; }

    [StringLength(450)]
    public string? UserId { get; set; }

    [Column("GradeID")]
    [StringLength(255)]
    public string? GradeId { get; set; }

    [StringLength(255)]
    public string? StdNationality { get; set; }

    [Column("SMS")]
    public int? Sms { get; set; }

    public int? BusNo { get; set; }

    public int? StdMobile { get; set; }

    [StringLength(255)]
    public string? Note { get; set; }

    [StringLength(255)]
    public string? OldSchool { get; set; }

    public int? StdComNo { get; set; }

    public int? StdSeat { get; set; }

    [StringLength(255)]
    public string? UserName { get; set; }

    public double? Mobile2 { get; set; }

    public int StdCode { get; set; }

    [Column("SSMA_TimeStamp")]
    public byte[] SsmaTimeStamp { get; set; } = null!;

    [ForeignKey("BusNo")]
    [InverseProperty("StdTables")]
    public virtual BusTable? BusNoNavigation { get; set; }

    [InverseProperty("Std")]
    public virtual ICollection<CousreDate> CousreDates { get; set; } = new List<CousreDate>();

    [InverseProperty("Std")]
    public virtual ICollection<ExamAb> ExamAbs { get; set; } = new List<ExamAb>();

    [ForeignKey("StdComNo")]
    [InverseProperty("StdTables")]
    public virtual Committee? StdComNoNavigation { get; set; }

    [ForeignKey("GradeId")]
    [InverseProperty("StdTables")]
    public virtual GradeTable? StdGradeNavigation { get; set; }
}
