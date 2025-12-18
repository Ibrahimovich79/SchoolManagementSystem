using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

[Table("CousreDate")]
[Index("StdId", Name = "CousreDate$StdID")]
public partial class CousreDate
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("cousreID")]
    public int? CousreId { get; set; }

    [Column("StdID")]
    public long? StdId { get; set; }

    [ForeignKey("CousreId")]
    [InverseProperty("CousreDates")]
    public virtual Course? Cousre { get; set; }

    [ForeignKey("StdId")]
    [InverseProperty("CousreDates")]
    public virtual StdTable? Std { get; set; }
}
