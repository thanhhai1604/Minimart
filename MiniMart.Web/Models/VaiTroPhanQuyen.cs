using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("VAITRO_PHANQUYEN")]
    public class VaiTroPhanQuyen
    {
        [Key, Column(Order = 0)]
        [StringLength(20)]
        public string MaVaiTro { get; set; } = null!;

        [Key, Column(Order = 1)]
        [StringLength(50)]
        public string MaPQ { get; set; } = null!;

        [ForeignKey("MaVaiTro")]
        public virtual VaiTro? VaiTro { get; set; }

        [ForeignKey("MaPQ")]
        public virtual PhanQuyen? PhanQuyen { get; set; }
    }
}