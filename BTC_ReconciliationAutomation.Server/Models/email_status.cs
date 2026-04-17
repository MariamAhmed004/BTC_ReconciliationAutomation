using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTC_ReconciliationAutomation.Server.Models;

[Table("email_status")]
public partial class email_status
{
    [Key]
    [Precision(8)]
    public int EMAIL_STATUS_ID { get; set; }

    [Column("EMAIL_STATUS")]
    [StringLength(50)]
    [Unicode(false)]
    public string? EMAIL_STATUS1 { get; set; }

    [InverseProperty("EMAIL_STATUS")]
    public virtual ICollection<generated_file> generated_files { get; set; } = new List<generated_file>();
}
