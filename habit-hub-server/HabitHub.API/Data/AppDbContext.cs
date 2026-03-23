using Microsoft.EntityFrameworkCore;
using HabitHub.Models.Entities;


namespace HabitHub.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<TeamCreator> TeamCreators { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<Habit> Habits { get; set; }
        public DbSet<HabitEntry> HabitEntries { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<InviteCode> InviteCodes { get; set; }
        public DbSet<TeamChat> TeamChats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Reminder> Reminders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .ToTable("Users")
                .HasDiscriminator<string>("UserType")
                .HasValue<TeamMember>("Member")
                .HasValue<TeamCreator>("Creator");


            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();


            modelBuilder.Entity<Membership>()
                .HasKey(m => new { m.UserId, m.TeamId });

            modelBuilder.Entity<Membership>()
                .HasOne(m => m.User)
                .WithMany(u => u.Memberships)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Membership>()
                .HasOne(m => m.Team)
                .WithMany(t => t.Memberships)
                .HasForeignKey(m => m.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Habit>()
                .HasOne(h => h.Team)
                .WithMany(t => t.Habits)
                .HasForeignKey(h => h.TeamId);

            modelBuilder.Entity<HabitEntry>()
                .HasOne(e => e.Habit)
                .WithMany(h => h.Entries)
                .HasForeignKey(e => e.HabitId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HabitEntry>()
                .HasOne(e => e.User)
                .WithMany(u => u.HabitEntries)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId);

            modelBuilder.Entity<InviteCode>()
                .HasIndex(i => i.Code)
                .IsUnique();

            modelBuilder.Entity<TeamChat>()
                .HasOne(c => c.Team)
                .WithOne(t => t.Chat)
                .HasForeignKey<TeamChat>(c => c.TeamId);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);     

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.Messages)         
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reminder>()
                .HasOne(r => r.Habit)
                .WithMany(h => h.Reminders)
                .HasForeignKey(r => r.HabitId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reminder>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reminders)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}