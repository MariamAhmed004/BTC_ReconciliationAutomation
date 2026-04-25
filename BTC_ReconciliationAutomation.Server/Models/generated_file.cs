using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTC_ReconciliationAutomation.Server.Models;

[Table("generated_file")]
[Index("RUN_ID", Name = "IXFK_generated_fi_reconci01")]
public partial class generated_file
{
    [Key]
    [Precision(8)]
    public int FILE_ID { get; set; }

    [StringLength(350)]
    [Unicode(false)]
    public string FILE_NAME { get; set; } = null!;

    [StringLength(1350)]
    [Unicode(false)]
    public string? SERVER_FILE_PATH { get; set; }

    [Precision(6)]
    public DateTime CREATED_AT { get; set; }

    [Precision(8)]
    public int RUN_ID { get; set; }

    [Column(TypeName = "NUMBER(38)")]
    public decimal FILE_TYPE_ID { get; set; }

    [ForeignKey("FILE_TYPE_ID")]
    [InverseProperty("generated_files")]
    public virtual file_type FILE_TYPE { get; set; } = null!;

    [ForeignKey("RUN_ID")]
    [InverseProperty("generated_files")]
    public virtual reconciliation_run RUN { get; set; } = null!;
}
