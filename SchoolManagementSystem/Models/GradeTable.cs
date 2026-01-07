using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

[Table("GradeTable")]
public partial class GradeTable
{
    [Key]
    [Column("GradeID")]
    [StringLength(255)]
    public string GradeId { get; set; } = null!;

    [StringLength(255)]
    public string? GradeName { get; set; }

    [InverseProperty("GradeNavigation")]
    public virtual ICollection<Relation> Relations { get; set; } = new List<Relation>();

    [InverseProperty("StdGradeNavigation")]
    public virtual ICollection<StdTable> StdTables { get; set; } = new List<StdTable>();
}
