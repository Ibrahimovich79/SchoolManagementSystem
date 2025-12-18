using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

[Table("2024-2025")]
public partial class _20242025
{
    [Key]
    [Column("QID")]
    public long Qid { get; set; }

    [StringLength(255)]
    public string? StdName { get; set; }

    [StringLength(255)]
    public string? OldBranche { get; set; }

    [StringLength(255)]
    public string? StdGrade { get; set; }

    [StringLength(255)]
    public string? StdNationality { get; set; }

    [Column("SMS")]
    public int? Sms { get; set; }

    [StringLength(255)]
    public string? Transport { get; set; }

    public int? BusNo { get; set; }

    public int? StdMobile { get; set; }

    [StringLength(255)]
    public string? Note { get; set; }

    [StringLength(255)]
    public string? OldSchool { get; set; }

    [StringLength(8000)]
    [Unicode(false)]
    public string? StdImage { get; set; }

    public int? StdComNo { get; set; }

    [StringLength(255)]
    public string? StdSeat { get; set; }

    [StringLength(255)]
    public string? UserName { get; set; }

    public double? F16 { get; set; }

    [StringLength(255)]
    public string? Field1 { get; set; }

    [StringLength(255)]
    public string? Field2 { get; set; }

    [Column("SSMA_TimeStamp")]
    public byte[] SsmaTimeStamp { get; set; } = null!;
}
