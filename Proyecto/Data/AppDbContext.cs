
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using proyecto.Models.FPY;
using proyecto.Models.Users;

namespace proyecto.Data
{
    /// <summary>
    /// This class allows to query data from an sqlite database.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public DbSet<UsersDiagnostico> UsersDiagnosticos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer("Server=FA00032VMA\\CONTIAGSPROD;Database=Continental.A_AN_O_AGS_QA;User Id=uif91872;Password=Lagordabee1!;default command timeout=600;")
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UsersDiagnostico>().ToTable("v_productdata");
            modelBuilder.Entity<UsersDiagnostico>(entity =>
            {
                entity.HasKey(e => e.UIE);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
