using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

[Table("LateTable")]
[Index("Id", Name = "LateTable$ID")]
public partial class LateTable
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("QID")]
    public long Qid { get; set; }

    [StringLength(255)]
    public string? StdName { get; set; }

    [StringLength(255)]
    public string? StdGrade { get; set; }

    [Column("SMS")]
    public int? Sms { get; set; }

    public int? StdMobile { get; set; }

    [StringLength(255)]
    public string? Note { get; set; }

    [Precision(0)]
    public DateTime? LateDate { get; set; }

    [Precision(0)]
    public DateTime? LateTime { get; set; }

    [Column("UserID")]
    [StringLength(255)]
    public string? UserId { get; set; }
}
