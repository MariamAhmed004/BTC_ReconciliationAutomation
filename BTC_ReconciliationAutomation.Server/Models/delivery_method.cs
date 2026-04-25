using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTC_ReconciliationAutomation.Server.Models;

[Table("delivery_method")]
public partial class delivery_method
{
    [Key]
    [Precision(8)]
    public int DELIVERY_METHOD_ID { get; set; }

    [Column("DELIVERY_METHOD")]
    [StringLength(150)]
    [Unicode(false)]
    public string DELIVERY_METHOD1 { get; set; } = null!;

    [InverseProperty("DELIVERY_METHOD")]
    public virtual ICollection<reconciliation_run> reconciliation_runs { get; set; } = new List<reconciliation_run>();
}
