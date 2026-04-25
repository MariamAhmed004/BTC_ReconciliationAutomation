using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTC_ReconciliationAutomation.Server.Models;

[Table("file_type")]
public partial class file_type
{
    [Key]
    [Column(TypeName = "NUMBER")]
    public decimal FILE_TYPE_ID { get; set; }

    [StringLength(150)]
    [Unicode(false)]
    public string FILE_TYPE_NAME { get; set; } = null!;

    [InverseProperty("FILE_TYPE")]
    public virtual ICollection<generated_file> generated_files { get; set; } = new List<generated_file>();
}
