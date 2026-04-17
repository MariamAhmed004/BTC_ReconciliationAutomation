using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTC_ReconciliationAutomation.Server.Models;

[Table("SIEBEL_TABLE")]
public partial class SIEBEL_TABLE
{
    [Key]
    [StringLength(50)]
    [Unicode(false)]
    public string BNET_REF { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string? PROD_LINE { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? RENTAL_PLAN { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? ACCNT_TYPE { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? SEG { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? SUB_SEG { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? CPS_NUM { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? CPS_IDENTIFIER { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? STATUS_CD { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? BNET_PACK_CODE { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? MEDIA { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? START_DT { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? DISCONNECT_DT { get; set; }
}
