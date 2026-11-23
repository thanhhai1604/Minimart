using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("PHANQUYEN")]
    public class PhanQuyen
    {
        [Key]
        [Column("MaPQ")]
        [StringLength(50)]
        public string MaPQ { get; set; } = string.Empty;

        [Required]
        [Column("TenPQ")]
        [StringLength(100)]
        public string TenPQ { get; set; } = string.Empty;

        [Column("NenTang")]
        [StringLength(50)]
        public string NenTang { get; set; } = string.Empty; // WEB hoặc POS

        [Column("QuyenCha")]
        [StringLength(50)]
        public string? QuyenCha { get; set; }

        [Column("MoTa")]
        [StringLength(255)]
        public string? MoTa { get; set; }
    }
}