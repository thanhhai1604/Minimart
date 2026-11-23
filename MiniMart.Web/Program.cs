using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MiniMart.Web.Data;
using MiniMart.Web.Models;

namespace MiniMart.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Kết nối cơ sở dữ liệu PostgreSQL (Supabase)
            builder.Services.AddDbContext<MiniMartDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2. Password hasher cho nhân viên
            builder.Services.AddScoped<IPasswordHasher<NhanVien>, PasswordHasher<NhanVien>>();

            // 3. Session lưu trong bộ nhớ
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // 4. MVC (Controllers và Views)
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // 5. Xử lý lỗi hệ thống (exception) khi chạy Production
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // 6. Bắt mã lỗi HTTP không có nội dung (404, 403...) và chạy lại action Home/NotFound/{mã lỗi}
            //    Đường dẫn sai trên thanh địa chỉ được giữ nguyên.
            app.UseStatusCodePagesWithReExecute("/Home/NotFound/{0}");

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
