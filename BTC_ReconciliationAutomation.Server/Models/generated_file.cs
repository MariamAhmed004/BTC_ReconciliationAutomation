using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTC_ReconciliationAutomation.Server.Models;

[Table("generated_file")]
[Index("DELIVERY_METHOD_ID", Name = "IXFK_generated_fi_deliver01")]
[Index("RUN_ID", Name = "IXFK_generated_fi_reconci01")]
[Index("EMAIL_STATUS_ID", Name = "IXFK_generated_file_email01")]
public partial class generated_file
{
    [Key]
    [Precision(8)]
    public int FILE_ID { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? FILE_NAME { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? SERVER_FILE_PATH { get; set; }

    [Precision(6)]
    public DateTime? CREATED_AT { get; set; }

    [Precision(8)]
    public int? DELIVERY_METHOD_ID { get; set; }

    [Precision(8)]
    public int? EMAIL_STATUS_ID { get; set; }

    [Precision(8)]
    public int? RUN_ID { get; set; }

    [ForeignKey("DELIVERY_METHOD_ID")]
    [InverseProperty("generated_files")]
    public virtual delivery_method? DELIVERY_METHOD { get; set; }

    [ForeignKey("EMAIL_STATUS_ID")]
    [InverseProperty("generated_files")]
    public virtual email_status? EMAIL_STATUS { get; set; }

    [ForeignKey("RUN_ID")]
    [InverseProperty("generated_files")]
    public virtual reconciliation_run? RUN { get; set; }
}
