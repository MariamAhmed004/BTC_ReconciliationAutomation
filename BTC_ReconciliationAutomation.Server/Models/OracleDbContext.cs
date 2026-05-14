using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BTC_ReconciliationAutomation.Server.Models;

public partial class OracleDbContext : DbContext
{
    private readonly IConfiguration? _configuration;

    public OracleDbContext()
    {
    }

    public OracleDbContext(DbContextOptions<OracleDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public virtual DbSet<ROWB_TABLE> ROWB_TABLEs { get; set; }

    public virtual DbSet<SIEBEL_TABLE> SIEBEL_TABLEs { get; set; }

    public virtual DbSet<delivery_method> delivery_methods { get; set; }

    public virtual DbSet<email_status> email_statuses { get; set; }

    public virtual DbSet<file_type> file_types { get; set; }

    public virtual DbSet<generated_file> generated_files { get; set; }

    public virtual DbSet<log_level> log_levels { get; set; }

    public virtual DbSet<reconciliation_run> reconciliation_runs { get; set; }

    public virtual DbSet<reconciliation_summary> reconciliation_summaries { get; set; }

    public virtual DbSet<run_status> run_statuses { get; set; }

    public virtual DbSet<system_configuration> system_configurations { get; set; }

    public virtual DbSet<system_log> system_logs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseOracle(_configuration?.GetConnectionString("OracleDb")
                ?? "User Id=billing_mock;Password=billing123;Data Source=localhost:1521/XEPDB1;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var schema = _configuration?["Oracle:Schema"] ?? "BILLING_MOCK";

        modelBuilder
            .HasDefaultSchema(schema)
            .UseCollation("USING_NLS_COMP");

        modelBuilder.Entity<ROWB_TABLE>(entity =>
        {
            entity.HasOne(d => d.BNET_REFNavigation).WithMany().HasConstraintName("FK_BNET_REF");
        });

        modelBuilder.Entity<SIEBEL_TABLE>(entity =>
        {
            entity.HasKey(e => e.BNET_REF).HasName("SYS_C008245");
        });

        modelBuilder.Entity<delivery_method>(entity =>
        {
            entity.HasKey(e => e.DELIVERY_METHOD_ID).HasName("PK_delivery_met_01");
        });

        modelBuilder.Entity<file_type>(entity =>
        {
            entity.HasKey(e => e.FILE_TYPE_ID).HasName("SYS_C008265");

            entity.Property(e => e.FILE_TYPE_ID).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<generated_file>(entity =>
        {
            entity.Property(e => e.FILE_TYPE_ID).HasDefaultValueSql("5\n   ");

            entity.HasOne(d => d.FILE_TYPE).WithMany(p => p.generated_files)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GENERATED_FILE_TYPE");

            entity.HasOne(d => d.RUN).WithMany(p => p.generated_files)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_generated_fi_reconciliat_01");
        });

        modelBuilder.Entity<reconciliation_run>(entity =>
        {
            entity.HasKey(e => e.RUN_ID).HasName("PK_reconciliation_log");

            entity.Property(e => e.DELIVERY_METHOD_ID).HasDefaultValueSql("1");
            entity.Property(e => e.RUN_STATUS_ID).HasDefaultValueSql("1\n   ");
            entity.Property(e => e.TRIGGERED_BY).HasDefaultValueSql("'Automatically'");

            entity.HasOne(d => d.CONFIG).WithMany(p => p.reconciliation_runs).HasConstraintName("FK_reconciliati_system_conf_01");

            entity.HasOne(d => d.DELIVERY_METHOD).WithMany(p => p.reconciliation_runs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RUN_DELIVERY_METHOD");

            entity.HasOne(d => d.EMAIL_STATUS).WithMany(p => p.reconciliation_runs).HasConstraintName("FK_RUN_EMAIL_STATUS");

            entity.HasOne(d => d.RUN_STATUS).WithMany(p => p.reconciliation_runs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RUN_RUN_STATUS");
        });

        modelBuilder.Entity<reconciliation_summary>(entity =>
        {
            entity.HasKey(e => e.SUMMARY_ID).HasName("PK_reconciliation_01");

            entity.Property(e => e.MISMATCH_COUNT).HasDefaultValueSql("0");
            entity.Property(e => e.MISSING_IN_BILLING_COUNT).HasDefaultValueSql("0");
            entity.Property(e => e.MISSING_IN_CUSTOMER_COUNT).HasDefaultValueSql("0");
            entity.Property(e => e.TOTAL_DISCREPANCIES).HasDefaultValueSql("0");
            entity.Property(e => e.TOTAL_RECORDS_PROCESSED).HasDefaultValueSql("0");

            entity.HasOne(d => d.RUN).WithMany(p => p.reconciliation_summaries)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_reconciliati_reconciliat_01");
        });

        modelBuilder.Entity<run_status>(entity =>
        {
            entity.HasKey(e => e.RUN_STATUS_ID).HasName("SYS_C008231");

            entity.Property(e => e.RUN_STATUS_ID).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<system_configuration>(entity =>
        {
            entity.HasKey(e => e.CONFIG_ID).HasName("PK_system_configur_01");

            entity.Property(e => e.FREQUENCY).HasDefaultValueSql("'MONTHLY'");
            entity.Property(e => e.IS_ACTIVE)
                .HasDefaultValueSql("'Y'")
                .IsFixedLength();
        });

        modelBuilder.Entity<system_log>(entity =>
        {
            entity.HasOne(d => d.LOG_LEVEL).WithMany(p => p.system_logs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_system_log_log_level");

            entity.HasOne(d => d.RUN).WithMany(p => p.system_logs).HasConstraintName("FK_system_log_reconciliatio_01");
        });
        modelBuilder.HasSequence("SEQ_file_type_FILE_TYPE_ID");
        modelBuilder.HasSequence("SEQ_run_status_RUN_STATUS_ID");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
