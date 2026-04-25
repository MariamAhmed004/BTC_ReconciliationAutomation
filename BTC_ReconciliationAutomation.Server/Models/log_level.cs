using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTC_ReconciliationAutomation.Server.Models;

[Table("log_level")]
public partial class log_level
{
    [Key]
    [Precision(8)]
    public int LOG_LEVEL_ID { get; set; }

    [Column("LOG_LEVEL")]
    [StringLength(100)]
    [Unicode(false)]
    public string LOG_LEVEL1 { get; set; } = null!;

    [InverseProperty("LOG_LEVEL")]
    public virtual ICollection<system_log> system_logs { get; set; } = new List<system_log>();
}
