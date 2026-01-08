using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Data
{
    public partial class SchoolDbContext : IdentityDbContext
    {
        public SchoolDbContext(DbContextOptions<SchoolDbContext> options)
            : base(options)
        {
        }

        // Add DbSets for your tables
        public virtual DbSet<StdTable> StdTables { get; set; }
        public virtual DbSet<TeacherTb> TeacherTbs { get; set; }
        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<BusTable> BusTables { get; set; }
        public virtual DbSet<Committee> Committees { get; set; }
        public virtual DbSet<GradeTable> GradeTables { get; set; }
        public virtual DbSet<Relation> Relations { get; set; }
        public virtual DbSet<CousreDate> CousreDates { get; set; }
        public virtual DbSet<ExamAb> ExamAbs { get; set; }
        public virtual DbSet<ExamStd> ExamStds { get; set; }
        public virtual DbSet<LateTable> LateTables { get; set; }
        public virtual DbSet<StudentAttendance> StudentAttendances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // CRITICAL: Call base.OnModelCreating needed for Identity
            base.OnModelCreating(modelBuilder);

            // Add any additional configuration here if needed
        }
    }
}