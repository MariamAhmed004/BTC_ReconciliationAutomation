using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTC_ReconciliationAutomation.Server.Models;

[Table("system_log")]
[Index("LOG_LEVEL_ID", Name = "IXFK_system_log_log_level")]
[Index("RUN_ID", Name = "IXFK_system_log_reconcili01")]
public partial class system_log
{
    [Key]
    [Precision(12)]
    public long LOG_ID { get; set; }

    [Unicode(false)]
    public string? LOG_MESSAGE { get; set; }

    [Precision(6)]
    public DateTime CREATED_AT { get; set; }

    [Precision(8)]
    public int LOG_LEVEL_ID { get; set; }

    [Precision(8)]
    public int? RUN_ID { get; set; }

    [ForeignKey("LOG_LEVEL_ID")]
    [InverseProperty("system_logs")]
    public virtual log_level LOG_LEVEL { get; set; } = null!;

    [ForeignKey("RUN_ID")]
    [InverseProperty("system_logs")]
    public virtual reconciliation_run? RUN { get; set; }
}
