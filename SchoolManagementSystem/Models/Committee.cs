using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

[Table("committee")]
public partial class Committee
{
    [Key]
    [Column("comNo")]
    public int ComNo { get; set; }

    [StringLength(255)]
    public string? ComSection { get; set; }

    [StringLength(255)]
    public string? Grade { get; set; }

    [Column("comLocation")]
    [StringLength(255)]
    public string? ComLocation { get; set; }

    public int? StdCount { get; set; }

    [Column("transport")]
    [StringLength(255)]
    public string? Transport { get; set; }

    public int? Mahroom { get; set; }

    public int? SpNeeds { get; set; }

    [InverseProperty("StdComNoNavigation")]
    public virtual ICollection<StdTable> StdTables { get; set; } = new List<StdTable>();
}
