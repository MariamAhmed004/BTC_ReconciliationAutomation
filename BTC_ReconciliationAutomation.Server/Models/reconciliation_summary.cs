using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTC_ReconciliationAutomation.Server.Models;

[Table("reconciliation_summary")]
[Index("RUN_ID", Name = "IXFK_reconciliati_reconci01")]
public partial class reconciliation_summary
{
    [Key]
    [Precision(8)]
    public int SUMMARY_ID { get; set; }

    [Column(TypeName = "NUMBER")]
    public decimal TOTAL_RECORDS_PROCESSED { get; set; }

    [Column(TypeName = "NUMBER")]
    public decimal TOTAL_DISCREPANCIES { get; set; }

    [Column(TypeName = "NUMBER")]
    public decimal MISMATCH_COUNT { get; set; }

    [Column(TypeName = "NUMBER")]
    public decimal MISSING_IN_CUSTOMER_COUNT { get; set; }

    [Column(TypeName = "NUMBER")]
    public decimal MISSING_IN_BILLING_COUNT { get; set; }

    [Precision(6)]
    public DateTime CREATED_AT { get; set; }

    [Precision(8)]
    public int RUN_ID { get; set; }

    [ForeignKey("RUN_ID")]
    [InverseProperty("reconciliation_summaries")]
    public virtual reconciliation_run RUN { get; set; } = null!;
}
