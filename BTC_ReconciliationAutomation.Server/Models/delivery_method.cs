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
    [StringLength(50)]
    [Unicode(false)]
    public string? DELIVERY_METHOD1 { get; set; }

    [InverseProperty("DELIVERY_METHOD")]
    public virtual ICollection<generated_file> generated_files { get; set; } = new List<generated_file>();
}
