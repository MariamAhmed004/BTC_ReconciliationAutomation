using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BTC_ReconciliationAutomation.Server.Models;

public partial class OracleDbContext : DbContext
{
    public OracleDbContext()
    {
    }

    public OracleDbContext(DbContextOptions<OracleDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ROWB_TABLE> ROWB_TABLEs { get; set; }

    public virtual DbSet<SIEBEL_TABLE> SIEBEL_TABLEs { get; set; }

    public virtual DbSet<delivery_method> delivery_methods { get; set; }

    public virtual DbSet<email_status> email_statuses { get; set; }

    public virtual DbSet<generated_file> generated_files { get; set; }

    public virtual DbSet<log_level> log_levels { get; set; }

    public virtual DbSet<reconciliation_run> reconciliation_runs { get; set; }

    public virtual DbSet<reconciliation_summary> reconciliation_summaries { get; set; }

    public virtual DbSet<system_configuration> system_configurations { get; set; }

    public virtual DbSet<system_log> system_logs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseOracle("User Id=billing_mock;Password=billing123;Data Source=localhost:1521/XEPDB1;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("BILLING_MOCK")
            .UseCollation("USING_NLS_COMP");

        modelBuilder.Entity<ROWB_TABLE>(entity =>
        {
            entity.HasOne(d => d.BNET_REFNavigation).WithMany().HasConstraintName("FK_BNET_REF");
        });

        modelBuilder.Entity<SIEBEL_TABLE>(entity =>
        {
            entity.HasKey(e => e.BNET_REF).HasName("SYS_C008232");
        });

        modelBuilder.Entity<delivery_method>(entity =>
        {
            entity.HasKey(e => e.DELIVERY_METHOD_ID).HasName("PK_delivery_met_01");
        });

        modelBuilder.Entity<generated_file>(entity =>
        {
            entity.HasOne(d => d.DELIVERY_METHOD).WithMany(p => p.generated_files).HasConstraintName("FK_generated_fi_delivery_me_01");

            entity.HasOne(d => d.EMAIL_STATUS).WithMany(p => p.generated_files).HasConstraintName("FK_generated_file_email_status");

            entity.HasOne(d => d.RUN).WithMany(p => p.generated_files).HasConstraintName("FK_generated_fi_reconciliat_01");
        });

        modelBuilder.Entity<reconciliation_run>(entity =>
        {
            entity.HasKey(e => e.RUN_ID).HasName("PK_reconciliation_log");

            entity.HasOne(d => d.CONFIG).WithMany(p => p.reconciliation_runs).HasConstraintName("FK_reconciliati_system_conf_01");
        });

        modelBuilder.Entity<reconciliation_summary>(entity =>
        {
            entity.HasKey(e => e.SUMMARY_ID).HasName("PK_reconciliation_01");

            entity.HasOne(d => d.RUN).WithMany(p => p.reconciliation_summaries).HasConstraintName("FK_reconciliati_reconciliat_01");
        });

        modelBuilder.Entity<system_configuration>(entity =>
        {
            entity.HasKey(e => e.CONFIG_ID).HasName("PK_system_configur_01");
        });

        modelBuilder.Entity<system_log>(entity =>
        {
            entity.HasOne(d => d.LOG_LEVEL).WithMany(p => p.system_logs).HasConstraintName("FK_system_log_log_level");

            entity.HasOne(d => d.RUN).WithMany(p => p.system_logs).HasConstraintName("FK_system_log_reconciliatio_01");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
