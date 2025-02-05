using proyecto.Models;
using proyecto.Models.Auth;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using proyecto.Models.Users;
using proyecto.Models.FPY;
using proyecto.Models.FPY.Db;

namespace proyecto.Data
{
    public class AnalysisDbContext : DbContext
    {
        public DbSet<Analysis> Analysis { get; set; }

        public DbSet<AnalysisAgs> AnalysisAgs { get; set; }

        public DbSet<Family> Family { get; set; }

        public DbSet<ProcessM> Process { get; set; }

        public DbSet<StationsFPYtest> StationsFPYtest { get; set; }

        public DbSet<UsersDiagnostico> UIE { get; set; }

        public DbSet<ReportFPYDB> ReporteFPY { get; set; }

        public DbSet<ReportFPYDBbyProcess> ReporteFPYbyProcess { get; set; }

        public DbSet<MinuteroFPY> MinuteroFPYs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(connectionString: "Filename=AnalysisDB.db;Password=Notelasabes.123!",
                sqliteOptionsAction: op =>
                {
                    op.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                });

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Family>().ToTable("t_bitDiag_Families");
            modelBuilder.Entity<Family>(entity =>
            {
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<ProcessM>().ToTable("v_bitDiag_processByProduct");
            modelBuilder.Entity<ProcessM>(entity =>
            {
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<Analysis>().ToTable("t_bitDiag_Analysis_nog");
            modelBuilder.Entity<Analysis>(entity =>
            {
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<AnalysisAgs>().ToTable("t_bitDiag_Analysis_ags");
            modelBuilder.Entity<AnalysisAgs>(entity =>
            {
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<StationsFPYtest>().ToTable("t_fpy_stations");
            modelBuilder.Entity<StationsFPYtest>(entity =>
            {
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<UsersDiagnostico>().ToTable("dbo.UIE");
            modelBuilder.Entity<UsersDiagnostico>(entity =>
            {
                entity.HasKey(e => e.Corto);
            });

            modelBuilder.Entity<ReportFPYDB>().ToTable("t_fpy_ReportFPY_ByDayAndProduct");
            modelBuilder.Entity<ReportFPYDB>(entity =>
            {
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<ReportFPYDBbyProcess>().ToTable("t_fpy_ReportFPY_ByDayAndProduct_ByProcess");
            modelBuilder.Entity<ReportFPYDBbyProcess>(entity =>
            {
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<MinuteroFPY>().ToTable("t_fpy_Espectacular_timer");
            modelBuilder.Entity<MinuteroFPY>(entity =>
            {
                entity.HasKey(e => e.ID);
            });


            base.OnModelCreating(modelBuilder);
        }



    }
}
