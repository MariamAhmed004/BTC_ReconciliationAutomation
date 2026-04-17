using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTC_ReconciliationAutomation.Server.Models;

[Table("system_configuration")]
public partial class system_configuration
{
    [Key]
    [Precision(8)]
    public int CONFIG_ID { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? EMAIL_RECIPIENTS { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? SCHEDULE_EXPRESSION { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? IS_ACTIVE { get; set; }

    [Precision(6)]
    public DateTime? EFFECTIVE_FROM { get; set; }

    [Precision(6)]
    public DateTime? EFFECTIVE_TO { get; set; }

    [Precision(6)]
    public DateTime? CREATED_AT { get; set; }

    [InverseProperty("CONFIG")]
    public virtual ICollection<reconciliation_run> reconciliation_runs { get; set; } = new List<reconciliation_run>();
}
