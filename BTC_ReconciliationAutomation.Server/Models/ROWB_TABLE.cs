using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTC_ReconciliationAutomation.Server.Models;

[Keyless]
[Table("ROWB_TABLE")]
public partial class ROWB_TABLE
{
    [StringLength(50)]
    [Unicode(false)]
    public string? BNET_REF { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ID { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? OPERATOR_NAME { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? PRODUCT_CODE { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? STATUS { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ORIGINAL_STATUS { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? BILLING_END_DT { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? ROWB_PACKAGE { get; set; }

    [ForeignKey("BNET_REF")]
    public virtual SIEBEL_TABLE? BNET_REFNavigation { get; set; }
}
