using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

[Table("BusTable")]
public partial class BusTable
{
    [Key]
    public int BusNo { get; set; }

    [StringLength(255)]
    public string? BusSupervisor { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public int? SupervisorMobile { get; set; }

    [Column("plateNo")]
    public int? PlateNo { get; set; }

    [InverseProperty("BusNoNavigation")]
    public virtual ICollection<StdTable> StdTables { get; set; } = new List<StdTable>();
}
