using KiraNet.GutsMvc.BBS.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace KiraNet.GutsMvc.BBS.Infrastructure
{
    public class GutsMvcDbContext : DbContext
    {
        public virtual DbSet<Bbs> Bbs { get; set; }
        public virtual DbSet<Log> Log { get; set; }
        public virtual DbSet<Reply> Reply { get; set; }
        public virtual DbSet<ReplyUser> ReplyUser { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<Topic> Topic { get; set; }
        public virtual DbSet<TopicStar> TopicStar { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserToRole> UserToRole { get; set; }
        public virtual DbSet<UserStar> UserStar { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        optionsBuilder.UseSqlServer(@"Integrated Security=SSPI;Persist Security Info=False;User ID=sa;Initial Catalog=BBS;Data Source=localhost");
        //    }
        //}

        

        public GutsMvcDbContext(DbContextOptions<GutsMvcDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bbs>(entity =>
            {
                entity.ToTable("BBS");

                entity.HasIndex(e => e.Bbsname)
                    .HasName("IX_BBS")
                    .IsUnique();

                entity.Property(e => e.BbscreateTime)
                    .HasColumnName("BBSCreateTime")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Bbsname)
                    .IsRequired()
                    .HasColumnName("BBSName")
                    .HasMaxLength(50);

                entity.Property(e => e.Bbstype).HasColumnName("BBSType")
                    .IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Bbs)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_BBS");
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.Property(e => e.Message)
                    .HasMaxLength(200)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Reply>(entity =>
            {
                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.TopicName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.ReplyType)
                    .IsRequired()
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.ReplyCount)
                    .IsRequired()
                    .HasDefaultValueSql("((0))");

                entity.HasOne(d => d.Topic)
                    .WithMany(p => p.Reply)
                    .HasForeignKey(d => d.TopicId)
                    .OnDelete(DeleteBehavior.Cascade)  // 设置级联删除
                    .HasConstraintName("FK_Topic_Reply");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Reply)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Reply");
            });

            modelBuilder.Entity<ReplyUser>(entity =>
            {
                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.ReplyType)
                   .IsRequired()
                   .HasDefaultValueSql("((0))");

                entity.HasOne(d => d.ReplyUserNavigation)
                    .WithMany(p => p.ReplyUserReplyUser)
                    .HasForeignKey(d => d.ReplyUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_ReplyUser_ReplyUserId");

                entity.HasOne(d => d.Topic)
                    .WithMany(p => p.ReplyUser)
                    .HasForeignKey(d => d.TopicId)
                    .OnDelete(DeleteBehavior.Cascade)  // 设置级联删除
                    .HasConstraintName("FK_Topic_ReplyUser");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ReplyUserUser)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_ReplyUser_UserId");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_RoleName")
                    .IsUnique();

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<Topic>(entity =>
            {
                entity.Property(e => e.BbsId).HasColumnName("BBSId");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                //entity.Property(e => e.TopicStatus)
                //    .IsRequired()
                //    .HasDefaultValueSql("((1))");

                entity.Property(e => e.LastReplyTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ReplyCount).HasDefaultValueSql("((0))");

                entity.Property(e => e.StarCount).HasDefaultValueSql("((0))");

                entity.Property(e => e.TopicName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Bbs)
                    .WithMany(p => p.Topic)
                    .HasForeignKey(d => d.BbsId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BBS_Topic");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Topic)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Topic");
            });

            modelBuilder.Entity<TopicStar>(entity =>
            {
                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TopicStar)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_TopicStar");

                entity.HasOne(d => d.Topic)
                    .WithMany(p => p.TopicStar)
                    .HasForeignKey(d => d.TopicId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Topic_TopicStar");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email)
                    .HasName("IX_User")
                    .IsUnique();

                entity.Property(e => e.City).HasMaxLength(50);
                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Sex)
                    .IsRequired()
                    .HasColumnType("bit")
                    .HasDefaultValueSql("((0))");
                entity.Property(e => e.HeadPhoto).HasMaxLength(200);
                entity.Property(e => e.Integrate).HasDefaultValueSql("((0))");
                entity.Property(e => e.Introduce).HasMaxLength(200);
                entity.Property(e => e.Nation).HasMaxLength(50);
                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RealName).HasMaxLength(20);

                entity.Property(e => e.Tel)
                    .HasMaxLength(11)
                    .IsUnicode(false);
                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<UserToRole>(entity =>
            {
                entity.ToTable("User_To_Role");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserToRole)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Role_User_Role");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserToRole)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_User_To_Role");
            });

            modelBuilder.Entity<UserStar>(entity =>
            {
                entity.ToTable("UserStar");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserStarUser)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_UserStar_UserId");


                entity.HasOne(d => d.StarUser)
                    .WithMany(p => p.UserStarUserStar)
                    .HasForeignKey(d => d.StarUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_UserStar_StarUserId");
            });

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.ToTable("Chat");

                entity.Property(e => e.CreateTime)
                     .HasColumnType("datetime")
                     .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Message)
                     .IsRequired()
                     .HasMaxLength(500);

                entity.Property(e => e.IsArrive)
                     .IsRequired()
                     .HasColumnType("bit")
                     .HasDefaultValueSql("((0))");

                entity.HasOne(d => d.User)
                     .WithMany(p => p.UserChat)
                     .HasForeignKey(d => d.UserId)
                     .OnDelete(DeleteBehavior.ClientSetNull)
                     .HasConstraintName("FK_Chat_User");

                entity.HasOne(d => d.TargetUser)
                    .WithMany(p => p.TargetUserChat)
                    .HasForeignKey(d => d.TargetUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Chat_TargetUser");
            });
        }
    }
}
