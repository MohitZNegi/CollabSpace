using Microsoft.EntityFrameworkCore;
using CollabSpace.Models;

namespace CollabSpace.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Workspace> Workspaces => Set<Workspace>();
        public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();
        public DbSet<Board> Boards => Set<Board>();
        public DbSet<Card> Cards => Set<Card>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<DirectMessage> DirectMessages => Set<DirectMessage>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- User ---
            modelBuilder.Entity<User>(e =>
            {
                e.HasIndex(u => u.Email).IsUnique();
                e.HasIndex(u => u.Username).IsUnique();
                e.HasIndex(u => u.LastSeenAt);
                e.Property(u => u.Username).HasMaxLength(50);
                e.Property(u => u.Email).HasMaxLength(255);
                e.Property(u => u.PasswordHash).HasMaxLength(255);
                e.Property(u => u.GlobalRole).HasMaxLength(20);
                e.Property(u => u.AvatarUrl).HasMaxLength(500);
            });

            // --- Workspace ---
            modelBuilder.Entity<Workspace>(e =>
            {
                e.HasIndex(w => w.InviteCode).IsUnique();
                e.Property(w => w.Name).HasMaxLength(100);
                e.Property(w => w.Description).HasMaxLength(500);
                e.Property(w => w.InviteCode).HasMaxLength(20);
                e.HasOne(w => w.Owner)
                    .WithMany(u => u.OwnedWorkspaces)
                    .HasForeignKey(w => w.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- WorkspaceMember ---
            modelBuilder.Entity<WorkspaceMember>(e =>
            {
                e.HasIndex(wm => new { wm.WorkspaceId, wm.UserId }).IsUnique();
                e.HasIndex(wm => wm.WorkspaceId);
                e.HasIndex(wm => wm.UserId);
                e.Property(wm => wm.WorkspaceRole).HasMaxLength(20);
                e.HasOne(wm => wm.Workspace)
                    .WithMany(w => w.Members)
                    .HasForeignKey(wm => wm.WorkspaceId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(wm => wm.User)
                    .WithMany(u => u.WorkspaceMemberships)
                    .HasForeignKey(wm => wm.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Board ---
            modelBuilder.Entity<Board>(e =>
            {
                e.HasIndex(b => b.WorkspaceId);
                e.Property(b => b.Name).HasMaxLength(100);
                e.HasOne(b => b.Workspace)
                    .WithMany(w => w.Boards)
                    .HasForeignKey(b => b.WorkspaceId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(b => b.CreatedBy)
                    .WithMany(u => u.CreatedBoards)
                    .HasForeignKey(b => b.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Card ---
            modelBuilder.Entity<Card>(e =>
            {
                e.HasIndex(c => c.BoardId);
                e.HasIndex(c => c.Status);
                e.HasIndex(c => c.Position);
                e.Property(c => c.Title).HasMaxLength(200);
                e.Property(c => c.Status).HasMaxLength(20);
                e.HasOne(c => c.Board)
                    .WithMany(b => b.Cards)
                    .HasForeignKey(c => c.BoardId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(c => c.AssignedTo)
                    .WithMany(u => u.AssignedCards)
                    .HasForeignKey(c => c.AssignedToUserId)
                    .OnDelete(DeleteBehavior.SetNull);
                e.HasOne(c => c.CreatedBy)
                    .WithMany(u => u.CreatedCards)
                    .HasForeignKey(c => c.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Message ---
            modelBuilder.Entity<Message>(e =>
            {
                e.HasIndex(m => m.WorkspaceId);
                e.HasIndex(m => m.SenderId);
                e.HasIndex(m => m.SentAt);
                e.HasOne(m => m.Workspace)
                    .WithMany(w => w.Messages)
                    .HasForeignKey(m => m.WorkspaceId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(m => m.Sender)
                    .WithMany(u => u.SentMessages)
                    .HasForeignKey(m => m.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- DirectMessage ---
            modelBuilder.Entity<DirectMessage>(e =>
            {
                e.HasIndex(dm => dm.SenderId);
                e.HasIndex(dm => dm.RecipientId);
                e.HasIndex(dm => dm.SentAt);
                e.HasOne(dm => dm.Sender)
                    .WithMany(u => u.SentDirectMessages)
                    .HasForeignKey(dm => dm.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(dm => dm.Recipient)
                    .WithMany(u => u.ReceivedDirectMessages)
                    .HasForeignKey(dm => dm.RecipientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Comment ---
            modelBuilder.Entity<Comment>(e =>
            {
                e.HasIndex(c => c.CardId);
                e.HasIndex(c => c.CreatedAt);
                e.HasOne(c => c.Card)
                    .WithMany(card => card.Comments)
                    .HasForeignKey(c => c.CardId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(c => c.ParentComment)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(c => c.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });

            // --- Notification ---
            modelBuilder.Entity<Notification>(e =>
            {
                e.HasIndex(n => n.RecipientUserId);
                e.HasIndex(n => n.Type);
                e.HasIndex(n => n.IsRead);
                e.HasIndex(n => n.CreatedAt);
                e.Property(n => n.Type).HasMaxLength(50);
                e.Property(n => n.Message).HasMaxLength(500);
                e.HasOne(n => n.Recipient)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.RecipientUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
