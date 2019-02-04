using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class Produit
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Libelle { get; set; }

        [Column(TypeName = "nvarchar(1000)")]
        //[MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "numeric(8, 2)")]
        public double Prix { get; set; }
    }
}
