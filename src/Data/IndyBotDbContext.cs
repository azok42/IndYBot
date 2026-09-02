using IndYBot.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IndYBot.Data;

public partial class IndyBotDbContext : DbContext
{
   private readonly string _connectionString;

   public IndyBotDbContext(IConfiguration config)
   {
      var connectionString = config["Database:Connection"];

      if (string.IsNullOrEmpty(connectionString))
         throw new ArgumentNullException("connectionString must not be null");

      _connectionString = connectionString;
   }

   public IndyBotDbContext(
         IConfiguration config,
         DbContextOptions<IndyBotDbContext> options)
      : base(options)
   {
      var connectionString = config["Database:Connection"];

      if (string.IsNullOrEmpty(connectionString))
         throw new ArgumentNullException("connectionString must not be null");

      _connectionString = connectionString;
   }

   public virtual DbSet<AutoEntry> AutoEntries { get; set; }

   public virtual DbSet<AutoEntryConfig> AutoEntryConfigs { get; set; }

   public virtual DbSet<AutoEntryHistory> AutoEntryHistories { get; set; }

   public virtual DbSet<Credential> Credentials { get; set; }

   public virtual DbSet<Group> Groups { get; set; }

   public virtual DbSet<GroupEntry> GroupEntries { get; set; }

   public virtual DbSet<GroupEntryMember> GroupEntryMembers { get; set; }

   public virtual DbSet<GroupEntryTemplate> GroupEntryTemplates { get; set; }

   public virtual DbSet<GroupInvite> GroupInvites { get; set; }

   public virtual DbSet<GroupMember> GroupMembers { get; set; }

   public virtual DbSet<Guild> Guilds { get; set; }

   public virtual DbSet<GuildFeature> GuildFeatures { get; set; }

   public virtual DbSet<User> Users { get; set; }

   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
      => optionsBuilder.UseMySql(
            _connectionString,
            Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.32-mariadb")
         );

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      modelBuilder
         .UseCollation("latin1_swedish_ci")
         .HasCharSet("latin1");

      modelBuilder.Entity<AutoEntry>(entity =>
         {
            entity.HasKey(e => new { e.UserId, e.IndyDay })
               .HasName("PRIMARY")
               .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("auto_entries");

            entity.Property(e => e.UserId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("user_id");
            entity.Property(e => e.IndyDay)
               .HasColumnType("tinyint(4)")
               .HasColumnName("indy_day");
            entity.Property(e => e.Description)
               .HasMaxLength(100)
               .HasColumnName("description");
            entity.Property(e => e.ExecutionDay)
               .HasColumnType("tinyint(4)")
               .HasColumnName("execution_day");
            entity.Property(e => e.Subject)
               .HasMaxLength(100)
               .HasColumnName("subject");
            entity.Property(e => e.Teacher)
               .HasMaxLength(100)
               .HasColumnName("teacher");

            entity.HasOne(d => d.User).WithMany(p => p.AutoEntries)
               .HasForeignKey(d => d.UserId)
               .HasConstraintName("auto_entries_ibfk_1");
         });

      modelBuilder.Entity<AutoEntryConfig>(entity =>
         {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("auto_entry_configs");

            entity.Property(e => e.UserId)
               .ValueGeneratedNever()
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("user_id");
            entity.Property(e => e.Enabled).HasColumnName("enabled");
            entity.Property(e => e.Notifications)
               .HasDefaultValueSql("'Always'")
               .HasColumnType("enum('Disabled','Always','On_Error')")
               .HasColumnName("notifications");
            entity.Property(e => e.Time)
               .HasColumnType("time")
               .HasColumnName("time");

            entity.HasOne(d => d.User).WithOne(p => p.AutoEntryConfig)
               .HasForeignKey<AutoEntryConfig>(d => d.UserId)
               .HasConstraintName("auto_entry_configs_ibfk_1");
         });

      modelBuilder.Entity<AutoEntryHistory>(entity =>
         {
            entity.HasKey(e => new { e.UserId, e.ExecutedAt })
               .HasName("PRIMARY")
               .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("auto_entry_history");

            entity.Property(e => e.UserId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("user_id");
            entity.Property(e => e.ExecutedAt)
               .HasMaxLength(6)
               .HasColumnName("executed_at");
            entity.Property(e => e.EntriesCreated)
               .HasColumnType("tinyint(4)")
               .HasColumnName("entries_created");
            entity.Property(e => e.Status)
               .HasColumnType("enum('Success','Failed','Skipped')")
               .HasColumnName("status");

            entity.HasOne(d => d.User).WithMany(p => p.AutoEntryHistories)
               .HasForeignKey(d => d.UserId)
               .HasConstraintName("auto_entry_history_ibfk_1");
         });

      modelBuilder.Entity<Credential>(entity =>
         {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("credentials");

            entity.Property(e => e.UserId)
               .ValueGeneratedNever()
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
               .HasDefaultValueSql("current_timestamp()")
               .HasColumnType("datetime")
               .HasColumnName("created_at");
            entity.Property(e => e.Password)
               .HasMaxLength(255)
               .HasColumnName("password");
            entity.Property(e => e.Username)
               .HasMaxLength(255)
               .HasColumnName("username");

            entity.HasOne(d => d.User).WithOne(p => p.Credential)
               .HasForeignKey<Credential>(d => d.UserId)
               .HasConstraintName("credentials_ibfk_1");
            });

      modelBuilder.Entity<Group>(entity =>
         {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("groups");

            entity.HasIndex(e => e.GuildId, "guild_id").IsUnique();

            entity.HasIndex(e => e.OwnerId, "owner_id").IsUnique();

            entity.Property(e => e.Id)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
               .HasDefaultValueSql("current_timestamp()")
               .HasColumnType("datetime")
               .HasColumnName("created_at");
            entity.Property(e => e.GuildId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("guild_id");
            entity.Property(e => e.Name)
               .HasMaxLength(100)
               .HasColumnName("name");
            entity.Property(e => e.OwnerId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("owner_id");
            entity.Property(e => e.RoleId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("role_id");

            entity.HasOne(d => d.Guild).WithOne(p => p.Group)
               .HasForeignKey<Group>(d => d.GuildId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("groups_ibfk_1");

            entity.HasOne(d => d.Owner).WithOne(p => p.Group)
               .HasForeignKey<Group>(d => d.OwnerId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("groups_ibfk_2");
         });

      modelBuilder.Entity<GroupEntry>(entity =>
         {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("group_entries");

            entity.HasIndex(e => e.CreatorId, "creator_id");

            entity.HasIndex(e => e.GroupId, "group_id");

            entity.Property(e => e.Id)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("id");
            entity.Property(e => e.ClosedAt)
               .HasColumnType("datetime")
               .HasColumnName("closed_at");
            entity.Property(e => e.CreatedId)
               .HasDefaultValueSql("current_timestamp()")
               .HasColumnType("datetime")
               .HasColumnName("created_id");
            entity.Property(e => e.CreatorId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("creator_id");
            entity.Property(e => e.Description)
               .HasMaxLength(255)
               .HasColumnName("description");
            entity.Property(e => e.EntryDate)
               .HasColumnType("datetime")
               .HasColumnName("entry_date");
            entity.Property(e => e.GroupId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("group_id");
            entity.Property(e => e.Hour3).HasColumnName("hour3");
            entity.Property(e => e.Hour4).HasColumnName("hour4");
            entity.Property(e => e.Status)
               .HasDefaultValueSql("'Open'")
               .HasColumnType("enum('Open','Closed')")
               .HasColumnName("status");
            entity.Property(e => e.Subject)
               .HasMaxLength(255)
               .HasColumnName("subject");
            entity.Property(e => e.Teacher)
               .HasMaxLength(255)
               .HasColumnName("teacher");

            entity.HasOne(d => d.Creator).WithMany(p => p.GroupEntries)
               .HasForeignKey(d => d.CreatorId)
               .HasConstraintName("group_entries_ibfk_2");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupEntries)
               .HasForeignKey(d => d.GroupId)
               .HasConstraintName("group_entries_ibfk_1");
         });

      modelBuilder.Entity<GroupEntryMember>(entity =>
         {
            entity.HasKey(e => new { e.GroupEntryId, e.UserId })
               .HasName("PRIMARY")
               .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("group_entry_members");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.GroupEntryId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("group_entry_id");
            entity.Property(e => e.UserId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("user_id");
            entity.Property(e => e.JoinedAt)
               .HasDefaultValueSql("current_timestamp()")
               .HasColumnType("datetime")
               .HasColumnName("joined_at");

            entity.HasOne(d => d.GroupEntry).WithMany(p => p.GroupEntryMembers)
               .HasForeignKey(d => d.GroupEntryId)
               .HasConstraintName("group_entry_members_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.GroupEntryMembers)
               .HasForeignKey(d => d.UserId)
               .HasConstraintName("group_entry_members_ibfk_1");
         });

      modelBuilder.Entity<GroupEntryTemplate>(entity =>
         {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("group_entry_templates");

            entity.HasIndex(e => e.CreatorId, "creator_id");

            entity.HasIndex(e => e.GroupId, "group_id");

            entity.Property(e => e.Id)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
               .HasDefaultValueSql("current_timestamp()")
               .HasColumnType("datetime")
               .HasColumnName("created_at");
            entity.Property(e => e.CreatorId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("creator_id");
            entity.Property(e => e.Description)
               .HasMaxLength(100)
               .HasColumnName("description");
            entity.Property(e => e.GroupId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("group_id");
            entity.Property(e => e.Hour3).HasColumnName("hour3");
            entity.Property(e => e.Hour4).HasColumnName("hour4");
            entity.Property(e => e.Name)
               .HasMaxLength(100)
               .HasColumnName("name");
            entity.Property(e => e.Subject)
               .HasMaxLength(100)
               .HasColumnName("subject");
            entity.Property(e => e.Teacher)
               .HasMaxLength(100)
               .HasColumnName("teacher");

            entity.HasOne(d => d.Creator).WithMany(p => p.GroupEntryTemplates)
               .HasForeignKey(d => d.CreatorId)
               .HasConstraintName("group_entry_templates_ibfk_2");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupEntryTemplates)
               .HasForeignKey(d => d.GroupId)
               .HasConstraintName("group_entry_templates_ibfk_1");
         });

      modelBuilder.Entity<GroupInvite>(entity =>
         {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("group_invites");

            entity.HasIndex(e => e.GroupId, "group_id").IsUnique();

            entity.HasIndex(e => e.InviteeId, "invitee_id").IsUnique();

            entity.HasIndex(e => e.InviterId, "inviter_id").IsUnique();

            entity.Property(e => e.Id)
               .ValueGeneratedNever()
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
               .HasDefaultValueSql("current_timestamp()")
               .HasColumnType("datetime")
               .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt)
               .HasColumnType("datetime")
               .HasColumnName("expires_at");
            entity.Property(e => e.GroupId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("group_id");
            entity.Property(e => e.InviteeId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("invitee_id");
            entity.Property(e => e.InviterId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("inviter_id");

            entity.HasOne(d => d.Group).WithOne(p => p.GroupInvite)
               .HasForeignKey<GroupInvite>(d => d.GroupId)
               .HasConstraintName("group_invites_ibfk_1");

            entity.HasOne(d => d.Invitee).WithOne(p => p.GroupInviteInvitee)
               .HasForeignKey<GroupInvite>(d => d.InviteeId)
               .HasConstraintName("group_invites_ibfk_3");

            entity.HasOne(d => d.Inviter).WithOne(p => p.GroupInviteInviter)
               .HasForeignKey<GroupInvite>(d => d.InviterId)
               .HasConstraintName("group_invites_ibfk_2");
         });

      modelBuilder.Entity<GroupMember>(entity =>
         {
            entity.HasKey(e => new { e.GroupId, e.UserId })
            .HasName("PRIMARY")
            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("group_members");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.GroupId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("group_id");
            entity.Property(e => e.UserId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("user_id");
            entity.Property(e => e.JoinedAt)
               .HasDefaultValueSql("current_timestamp()")
               .HasColumnType("datetime")
               .HasColumnName("joined_at");
            entity.Property(e => e.Role)
               .HasDefaultValueSql("'Member'")
               .HasColumnType("enum('Member','Moderator','Owner')")
               .HasColumnName("role");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupMembers)
               .HasForeignKey(d => d.GroupId)
               .HasConstraintName("group_members_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.GroupMembers)
               .HasForeignKey(d => d.UserId)
               .HasConstraintName("group_members_ibfk_2");
         });

      modelBuilder.Entity<Guild>(entity =>
         {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("guilds");

            entity.HasIndex(e => e.DiscordId, "discord_id").IsUnique();

            entity.Property(e => e.Id)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
               .HasDefaultValueSql("current_timestamp()")
               .HasColumnType("datetime")
               .HasColumnName("created_at");
            entity.Property(e => e.DiscordId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("discord_id");
            entity.Property(e => e.Timezone)
               .HasMaxLength(64)
               .HasDefaultValueSql("'Europe/Vienna'")
               .HasColumnName("timezone");

            entity.HasMany(d => d.Users)
               .WithMany(p => p.Guilds)
               .UsingEntity<Dictionary<string, object>>(
                  "GuildAdmin",
                  r => r.HasOne<User>()
                        .WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("guild_admins_ibfk_2"),
                  l => l.HasOne<Guild>().WithMany()
                        .HasForeignKey("GuildId")
                        .HasConstraintName("guild_admins_ibfk_1"),
                  j =>
                  {
                     j.HasKey("GuildId", "UserId")
                        .HasName("PRIMARY")
                        .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                     j.ToTable("guild_admins");
                     j.HasIndex(new[] { "UserId" }, "user_id");
                     j.IndexerProperty<ulong>("GuildId")
                        .HasColumnType("bigint(20) unsigned")
                        .HasColumnName("guild_id");
                     j.IndexerProperty<ulong>("UserId")
                        .HasColumnType("bigint(20) unsigned")
                        .HasColumnName("user_id");
                  }
               );
         });

      modelBuilder.Entity<GuildFeature>(entity =>
         {
            entity.HasKey(e => new { e.GuildId, e.Feature })
               .HasName("PRIMARY")
               .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("guild_features");

            entity.Property(e => e.GuildId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("guild_id");
            entity.Property(e => e.Feature)
               .HasMaxLength(64)
               .HasColumnName("feature");
            entity.Property(e => e.Enabled)
               .IsRequired()
               .HasDefaultValueSql("'1'")
               .HasColumnName("enabled");

            entity.HasOne(d => d.Guild).WithMany(p => p.GuildFeatures)
               .HasForeignKey(d => d.GuildId)
               .HasConstraintName("guild_features_ibfk_1");
         });

      modelBuilder.Entity<User>(entity =>
         {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.DiscordId, "discord_id")
               .IsUnique();

            entity.Property(e => e.Id)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
               .HasDefaultValueSql("current_timestamp()")
               .HasColumnType("datetime")
               .HasColumnName("created_at");
            entity.Property(e => e.DiscordId)
               .HasColumnType("bigint(20) unsigned")
               .HasColumnName("discord_id");
            entity.Property(e => e.WhereisEnabled)
               .HasColumnName("whereis_enabled");
            entity.Property(e => e.WhereisVisibility)
               .HasDefaultValueSql("'Group'")
               .HasColumnType("enum('Group','Server')")
               .HasColumnName("whereis_visibility");
         });

      OnModelCreatingPartial(modelBuilder);
   }

   partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
