using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

[Table("ExamStd")]
public partial class ExamStd
{
    [Key]
    [Column("StdID")]
    public long StdId { get; set; }

    [StringLength(255)]
    public string? StdName { get; set; }

    [StringLength(255)]
    public string? StdGrade { get; set; }

    [Column("G#")]
    public int? G { get; set; }

    public int? StdMobile { get; set; }

    [StringLength(255)]
    public string? StdTransport { get; set; }

    [StringLength(255)]
    public string? StdSeat { get; set; }

    public int? StdComNo { get; set; }

    [StringLength(255)]
    public string? Note { get; set; }
}
