using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace PcrSarenBot.DatabaseModels
{
    public partial class Saren_BotContext : DbContext
    {
        public Saren_BotContext()
        {
        }

        public Saren_BotContext(DbContextOptions<Saren_BotContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Battles> Battles { get; set; }
        public virtual DbSet<Boss> Boss { get; set; }
        public virtual DbSet<Guides> Guides { get; set; }
        public virtual DbSet<Member> Member { get; set; }
        public virtual DbSet<Parameters> Parameters { get; set; }
        public virtual DbSet<SlRecord> SlRecord { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

                optionsBuilder.UseMySql(Startup.SarenDbConnectionString, x => x.ServerVersion("5.7.26-mysql"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Battles>(entity =>
            {
                entity.HasKey(e => e.BattleId)
                    .HasName("PRIMARY");

                entity.HasComment("This table contains every battle record of each clan battle");

                entity.HasIndex(e => e.BattleId)
                    .HasName("battle_id_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.BossId)
                    .HasName("boss_code_idx");

                entity.HasIndex(e => e.MemberId)
                    .HasName("member_id_idx");

                entity.HasIndex(e => e.PlayerId)
                    .HasName("player_id_idx");

                entity.Property(e => e.BattleId)
                    .HasColumnName("battle_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BossId)
                    .HasColumnName("boss_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CycleNumber)
                    .HasColumnName("cycle_number")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Damage)
                    .HasColumnName("damage")
                    .HasColumnType("int(11)");

                entity.Property(e => e.EventId)
                    .HasColumnName("event_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.MemberId)
                    .HasColumnName("member_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.PlayerId)
                    .HasColumnName("player_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.RecordTime)
                    .HasColumnName("record_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.HasOne(d => d.Boss)
                    .WithMany(p => p.Battles)
                    .HasForeignKey(d => d.BossId)
                    .HasConstraintName("boss_code");

                entity.HasOne(d => d.Member)
                    .WithMany(p => p.BattlesMember)
                    .HasForeignKey(d => d.MemberId)
                    .HasConstraintName("member_id");

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.BattlesPlayer)
                    .HasForeignKey(d => d.PlayerId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("player_id");
            });

            modelBuilder.Entity<Boss>(entity =>
            {
                entity.HasComment("This table contains basic information for each clan battle bosses.");

                entity.HasIndex(e => e.BossId)
                    .HasName("boss_id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.BossId)
                    .HasColumnName("boss_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BossCode)
                    .HasColumnName("boss_code")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.BossName)
                    .HasColumnName("boss_name")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.EventId)
                    .HasColumnName("event_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.HealthPool)
                    .HasColumnName("health_pool")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<Guides>(entity =>
            {
                entity.HasKey(e => e.GuideId)
                    .HasName("PRIMARY");

                entity.HasComment("This table contains information of every uploaded clan battle guide.");

                entity.HasIndex(e => e.BossId)
                    .HasName("guide_boss_id_idx");

                entity.HasIndex(e => e.EventId)
                    .HasName("guide_event_id_idx");

                entity.HasIndex(e => e.GuideId)
                    .HasName("guide_id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.GuideId)
                    .HasColumnName("guide_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BossId)
                    .HasColumnName("boss_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Comment)
                    .HasColumnName("comment")
                    .HasColumnType("text")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.EventId)
                    .HasColumnName("event_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ImageUrl)
                    .HasColumnName("image_url")
                    .HasColumnType("varchar(80)")
                    .HasCharSet("latin1")
                    .HasCollation("latin1_swedish_ci");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("varchar(45)")
                    .HasDefaultValueSql("'Active'")
                    .HasCharSet("latin1")
                    .HasCollation("latin1_swedish_ci");

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(d => d.Boss)
                    .WithMany(p => p.Guides)
                    .HasForeignKey(d => d.BossId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("guide_boss_id");

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.Guides)
                    .HasForeignKey(d => d.EventId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("guide_event_id");
            });

            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasComment("This tables contains information for all the clan members.");

                entity.Property(e => e.MemberId)
                    .HasColumnName("member_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Nickname)
                    .HasColumnName("nickname")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Role)
                    .HasColumnName("role")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<Parameters>(entity =>
            {
                entity.HasKey(e => e.EventId)
                    .HasName("PRIMARY");

                entity.HasComment("This table contains parameters for the onging clan battle");

                entity.HasIndex(e => e.EventId)
                    .HasName("event_id_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.FirstBossId)
                    .HasName("first_boss_id_idx");

                entity.HasIndex(e => e.LastBossId)
                    .HasName("last_boss_id_idx");

                entity.Property(e => e.EventId)
                    .HasColumnName("event_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CurrentBossHealth)
                    .HasColumnName("current_boss_health")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CurrentBossId)
                    .HasColumnName("current_boss_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CurrentCycle)
                    .HasColumnName("current_cycle")
                    .HasColumnType("int(11)");

                entity.Property(e => e.EventEndTime)
                    .HasColumnName("event_end_time")
                    .HasColumnType("datetime");

                entity.Property(e => e.EventName)
                    .HasColumnName("event_name")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.EventStartTime)
                    .HasColumnName("event_start_time")
                    .HasColumnType("datetime");

                entity.Property(e => e.FirstBossId)
                    .HasColumnName("first_boss_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LastBossId)
                    .HasColumnName("last_boss_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.MaxRunningMemberAllowed)
                    .HasColumnName("max_running_member_allowed")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.HasOne(d => d.FirstBoss)
                    .WithMany(p => p.ParametersFirstBoss)
                    .HasForeignKey(d => d.FirstBossId)
                    .HasConstraintName("first_boss_id");

                entity.HasOne(d => d.LastBoss)
                    .WithMany(p => p.ParametersLastBoss)
                    .HasForeignKey(d => d.LastBossId)
                    .HasConstraintName("last_boss_id");
            });

            modelBuilder.Entity<SlRecord>(entity =>
            {
                entity.HasKey(e => e.RecordId)
                    .HasName("PRIMARY");

                entity.ToTable("SL_Record");

                entity.HasComment("This table is used for saving SL records");

                entity.HasIndex(e => e.MemberId)
                    .HasName("member_id_sl_idx");

                entity.HasIndex(e => e.RecordId)
                    .HasName("record_id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.RecordId)
                    .HasColumnName("record_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.MemberId)
                    .HasColumnName("member_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.RecordTime)
                    .HasColumnName("record_time")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.Member)
                    .WithMany(p => p.SlRecord)
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("member_id_sl");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
