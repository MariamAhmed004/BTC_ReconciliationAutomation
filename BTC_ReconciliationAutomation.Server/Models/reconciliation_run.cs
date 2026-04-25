using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTC_ReconciliationAutomation.Server.Models;

[Table("reconciliation_run")]
[Index("CONFIG_ID", Name = "IXFK_reconciliati_system_01")]
public partial class reconciliation_run
{
    [Key]
    [Precision(8)]
    public int RUN_ID { get; set; }

    [Precision(6)]
    public DateTime RUN_DATE { get; set; }

    [Precision(8)]
    public int? CONFIG_ID { get; set; }

    [StringLength(2500)]
    [Unicode(false)]
    public string? ERROR_MESSAGE { get; set; }

    [StringLength(250)]
    [Unicode(false)]
    public string TRIGGERED_BY { get; set; } = null!;

    [Precision(8)]
    public int DELIVERY_METHOD_ID { get; set; }

    [Precision(8)]
    public int? EMAIL_STATUS_ID { get; set; }

    [Column(TypeName = "NUMBER(38)")]
    public decimal RUN_STATUS_ID { get; set; }

    [ForeignKey("CONFIG_ID")]
    [InverseProperty("reconciliation_runs")]
    public virtual system_configuration? CONFIG { get; set; }

    [ForeignKey("DELIVERY_METHOD_ID")]
    [InverseProperty("reconciliation_runs")]
    public virtual delivery_method DELIVERY_METHOD { get; set; } = null!;

    [ForeignKey("EMAIL_STATUS_ID")]
    [InverseProperty("reconciliation_runs")]
    public virtual email_status? EMAIL_STATUS { get; set; }

    [ForeignKey("RUN_STATUS_ID")]
    [InverseProperty("reconciliation_runs")]
    public virtual run_status RUN_STATUS { get; set; } = null!;

    [InverseProperty("RUN")]
    public virtual ICollection<generated_file> generated_files { get; set; } = new List<generated_file>();

    [InverseProperty("RUN")]
    public virtual ICollection<reconciliation_summary> reconciliation_summaries { get; set; } = new List<reconciliation_summary>();

    [InverseProperty("RUN")]
    public virtual ICollection<system_log> system_logs { get; set; } = new List<system_log>();
}
