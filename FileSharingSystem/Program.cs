using FileSharingSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using FileSharingSystem.Services;
using IEmailSender = FileSharingSystem.Services.IEmailSender;


var builder = WebApplication.CreateBuilder(args);

// Đường dẫn lưu trữ tệp đã tải lên
string fileStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadedFiles");


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

builder.Services.AddScoped<IFileService>(provider =>
{
    var context = provider.GetRequiredService<ApplicationDbContext>(); // Lấy ApplicationDbContext từ DI container
    return new FileService(fileStoragePath, context); // Khởi tạo FileService với cả fileStoragePath và context
});

// Cấu hình Authentication và Authorization
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();



void ConfigureServices(IServiceCollection services)
{
    services.AddMemoryCache();
}




builder.Services.AddLogging();

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
        options.SignIn.RequireConfirmedAccount = false; 
        options.Password.RequiredLength = 6; 
    })
    .AddEntityFrameworkStores<ApplicationDbContext>(); // Sử dụng EF Core để lưu trữ dữ liệu
}

void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint(); 
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
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
