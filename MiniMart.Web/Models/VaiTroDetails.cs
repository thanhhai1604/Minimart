namespace MiniMart.Web.Models
{
    public class VaiTroDetails
    {
        public string MaVaiTro { get; set; } = string.Empty;
        public string TenVaiTro { get; set; } = string.Empty;
        public string? MoTa { get; set; }

        public List<NhanVienThuocVaiTro> DanhSachNhanVien { get; set; } = new List<NhanVienThuocVaiTro>();
        public List<PhanQuyenCheckbox> DanhSachQuyen { get; set; } = new List<PhanQuyenCheckbox>();
        public List<string> SelectedQuyenIds { get; set; } = new List<string>();
    }

    public class NhanVienThuocVaiTro
    {
        public string MaNV { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string TenDN { get; set; } = string.Empty;
        public string SoDT { get; set; } = string.Empty;

        // Tên siêu thị trực thuộc (null nếu nhân viên thuộc tất cả siêu thị)
        public string? TenST { get; set; }

        // 1 = Đang làm việc, 0 = Ngừng làm việc
        public int TrangThai { get; set; }
    }
}
