using FileSharingSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Đường dẫn lưu trữ tệp đã tải lên
string fileStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

// Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Cấu hình DbContext sử dụng MySQL
ConfigureDatabase(builder.Services, connectionString);

// Cấu hình Identity cho quản lý người dùng
ConfigureIdentity(builder.Services);

// Đăng ký dịch vụ MVC và Razor Runtime Compilation
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddDatabaseDeveloperPageExceptionFilter(); // Bộ lọc lỗi cho nhà phát triển

// Đăng ký dịch vụ FileService
builder.Services.AddScoped<IFileService>(provider => new FileService(fileStoragePath));

// Cấu hình Authentication và Authorization
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Xây dựng ứng dụng
var app = builder.Build();

// Cấu hình middleware
ConfigureMiddleware(app);

// Định nghĩa tuyến đường mặc định
ConfigureRoutes(app);

// Chạy ứng dụng
app.Run();

// Các phương thức cấu hình riêng biệt
void ConfigureDatabase(IServiceCollection services, string connectionString)
{
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 40))));
}

void ConfigureIdentity(IServiceCollection services)
{
    services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false; // Không yêu cầu xác nhận email
        options.Password.RequiredLength = 6; // Mật khẩu tối thiểu 6 ký tự
    })
    .AddEntityFrameworkStores<ApplicationDbContext>(); // Sử dụng EF Core để lưu trữ dữ liệu
}

void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint(); // Cho phép chạy migration trong môi trường phát triển
    }
    else
    {
        app.UseExceptionHandler("/Home/Error"); // Trang lỗi cho môi trường sản xuất
        app.UseHsts(); // Bảo mật HTTPS
    }

    app.UseHttpsRedirection(); // Chuyển hướng HTTP sang HTTPS
    app.UseStaticFiles(); // Sử dụng tệp tĩnh

    app.UseRouting(); // Kích hoạt routing
    app.UseAuthentication(); // Sử dụng xác thực
    app.UseAuthorization(); // Sử dụng phân quyền
}

void ConfigureRoutes(WebApplication app)
{
    // Định nghĩa tuyến đường mặc định cho ứng dụng MVC
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Bật Razor Pages nếu sử dụng
    app.MapRazorPages();
}
