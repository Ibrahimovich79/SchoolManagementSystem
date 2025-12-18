using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

[Keyless]
[Table("Paste Errors")]
public partial class PasteError
{
    [StringLength(255)]
    public string? F1 { get; set; }

    [StringLength(255)]
    public string? F2 { get; set; }
}
