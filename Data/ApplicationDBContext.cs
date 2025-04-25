using Microsoft.EntityFrameworkCore;
using NutriTrackAPI.Models;

namespace NutriTrack.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Consultant> Consultants { get; set; }
        public DbSet<ConsultantNote> ConsultantNotes { get; set; }
        public DbSet<ConsultantRequest> ConsultantRequests { get; set; }
        public DbSet<ExerciseEntry> ExerciseEntries { get; set; }
        public DbSet<MealEntry> MealEntries { get; set; }
        public DbSet<StreakHistory> StreakHistories { get; set; }
        public DbSet<UserConsultant> UserConsultants { get; set; }
        public DbSet<UserGoal> UserGoals { get; set; }
        public DbSet<WeightMeasurement> WeightMeasurements { get; set; }
        public DbSet<WaterIntake> WaterIntakes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserConsultant>()
                .HasKey(uc => new { uc.user_uid, uc.consultant_uid });

            modelBuilder.Entity<UserConsultant>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserConsultants)
                .HasForeignKey(uc => uc.user_uid);

            modelBuilder.Entity<UserConsultant>()
                .HasOne(uc => uc.Consultant)
                .WithMany(c => c.UserConsultants)
                .HasForeignKey(uc => uc.consultant_uid);

            modelBuilder.Entity<ConsultantNote>()
                .HasOne(cn => cn.User)
                .WithMany(u => u.ConsultantNotes)
                .HasForeignKey(cn => cn.user_uid)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ConsultantNote>()
                .HasOne(cn => cn.Consultant)
                .WithMany(c => c.ConsultantNotes)
                .HasForeignKey(cn => cn.consultant_uid)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ConsultantNote>()
                .HasOne(cn => cn.UserGoal)
                .WithMany(ug => ug.ConsultantNotes)
                .HasForeignKey(cn => cn.goal_id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ConsultantRequest>()
                .HasOne(cr => cr.User)
                .WithMany(u => u.ConsultantRequests)
                .HasForeignKey(cr => cr.user_uid);

            modelBuilder.Entity<ConsultantRequest>()
                .HasOne(cr => cr.Consultant)
                .WithMany(c => c.ConsultantRequests)
                .HasForeignKey(cr => cr.consultant_uid);

            modelBuilder.Entity<MealEntry>()
                .HasOne(me => me.User)
                .WithMany(u => u.MealEntries)
                .HasForeignKey(me => me.user_uid);

            modelBuilder.Entity<ExerciseEntry>()
                .HasOne(ee => ee.User)
                .WithMany(u => u.ExerciseEntries)
                .HasForeignKey(ee => ee.user_uid);

            modelBuilder.Entity<StreakHistory>()
                .HasOne(sh => sh.User)
                .WithMany(u => u.StreakHistories)
                .HasForeignKey(sh => sh.user_uid);

            modelBuilder.Entity<WeightMeasurement>()
                .HasOne(wm => wm.User)
                .WithMany(u => u.WeightMeasurements)
                .HasForeignKey(wm => wm.user_uid);

            modelBuilder.Entity<WaterIntake>()
                .HasOne(wi => wi.User)
                .WithMany(u => u.WaterIntakes)
                .HasForeignKey(wi => wi.user_uid);
        }
    }
}