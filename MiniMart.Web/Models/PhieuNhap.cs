using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("PHIEUNHAP")]
    public class PhieuNhap
    {
        [Key]
        [Column("MaPN")]
        [Required(ErrorMessage = "Mã phiếu nhập không được để trống.")]
        [StringLength(30)]
        [Display(Name = "Mã phiếu nhập")]
        public string MaPN { get; set; } = string.Empty;

        [Column("NgayNhap")]
        [Required(ErrorMessage = "Ngày nhập không được để trống.")]
        [Display(Name = "Ngày nhập")]
        public DateTime NgayNhap { get; set; } = DateTime.Now;

        [Column("MaNCC")]
        [Required(ErrorMessage = "Vui lòng chọn nhà cung cấp.")]
        [StringLength(20)]
        [Display(Name = "Nhà cung cấp")]
        public string MaNCC { get; set; } = string.Empty;

        [Column("MaST")]
        [Required(ErrorMessage = "Vui lòng chọn siêu thị.")]
        [StringLength(20)]
        [Display(Name = "Siêu thị")]
        public string MaST { get; set; } = string.Empty;

        [Column("MaNV")]
        [StringLength(50)]
        [Display(Name = "Nhân viên lập")]
        public string? MaNV { get; set; }

        [Column("TrangThai")]
        [Display(Name = "Trạng thái")]
        public int TrangThai { get; set; } = 0;

        [ForeignKey("MaNCC")]
        public virtual NhaCungCap? NhaCungCapNavigation { get; set; }

        [ForeignKey("MaST")]
        public virtual SieuThi? SieuThiNavigation { get; set; }

        [ForeignKey("MaNV")]
        public virtual NhanVien? NhanVienNavigation { get; set; }

        public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();
    }
}