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
    public DateTime? RUN_DATE { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? STATUS { get; set; }

    [Precision(8)]
    public int? CONFIG_ID { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ERROR_MESSAGE { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? TRIGGERED_BY { get; set; }

    [ForeignKey("CONFIG_ID")]
    [InverseProperty("reconciliation_runs")]
    public virtual system_configuration? CONFIG { get; set; }

    [InverseProperty("RUN")]
    public virtual ICollection<generated_file> generated_files { get; set; } = new List<generated_file>();

    [InverseProperty("RUN")]
    public virtual ICollection<reconciliation_summary> reconciliation_summaries { get; set; } = new List<reconciliation_summary>();

    [InverseProperty("RUN")]
    public virtual ICollection<system_log> system_logs { get; set; } = new List<system_log>();
}
