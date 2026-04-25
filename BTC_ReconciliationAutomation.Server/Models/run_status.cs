using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTC_ReconciliationAutomation.Server.Models;

[Table("run_status")]
public partial class run_status
{
    [Key]
    [Column(TypeName = "NUMBER")]
    public decimal RUN_STATUS_ID { get; set; }

    [Column("RUN_STATUS")]
    [StringLength(50)]
    [Unicode(false)]
    public string RUN_STATUS1 { get; set; } = null!;

    [InverseProperty("RUN_STATUS")]
    public virtual ICollection<reconciliation_run> reconciliation_runs { get; set; } = new List<reconciliation_run>();
}
