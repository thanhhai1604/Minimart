namespace MiniMart.Web.Models
{
    public class PhanQuyenCheckbox
    {
        public string MaPQ { get; set; } = string.Empty;
        public string TenPQ { get; set; } = string.Empty;
        public string NenTang { get; set; } = string.Empty; // WEB hoặc POS
        public string? QuyenCha { get; set; }
        public string? MoTa { get; set; }
        public bool DaChon { get; set; }
        public List<PhanQuyenCheckbox> Children { get; set; } = new List<PhanQuyenCheckbox>();
    }

    public class VaiTroViewModel
    {
        public string MaVaiTro { get; set; } = string.Empty;
        public string TenVaiTro { get; set; } = string.Empty;
        public List<PhanQuyenCheckbox> DanhSachQuyen { get; set; } = new List<PhanQuyenCheckbox>();
    }
}