using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

public partial class User
{
    [Key]
    [Column("username")]
    [StringLength(255)]
    public string Username { get; set; } = null!;

    [Column("password")]
    [StringLength(255)]
    public string? Password { get; set; }

    [Column("role")]
    [StringLength(255)]
    public string? Role { get; set; }
}
