using CSI402_Project_final.Data;
using CSI402_Project_final.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute());
});
builder.Services.AddHttpContextAccessor();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<PetShopDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)));

builder.Services.AddScoped<PetShopRepository>();
builder.Services.AddSession();
builder.Services.AddSignalR();

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// เชื่อมต่อและตรวจสอบ/Seed ข้อมูลเริ่มต้นลง MySQL (csi402db)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<PetShopDbContext>();
        PetShopDbInitializer.Initialize(db);
    }
}
catch (Exception ex)
{
    app.Logger.LogWarning("ไม่สามารถเชื่อมต่อฐานข้อมูลในขั้นตอนเริ่มระบบได้: {Message} (ระบบยังคงทำงานต่อเพื่อไม่ให้เว็บดับ)", ex.Message);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthentication();

// Multi-Role & Simultaneous Tab Access Middleware:
// แยก Session และสิทธิ์การทำงานของแต่ละแท็บอย่างแท้จริง
// แม้อีกแท็บหนึ่งจะล็อกอินเป็น Customer ก็จะไม่ทำให้แท็บ Back-Office (Admin/Owner/Staff) หลุดหรือถูก Access Denied
app.Use(async (context, next) =>
{
    // 1. ตรวจสอบ Role จาก Query String (?role=... หรือ ?asRole=...)
    string? roleParam = context.Request.Query["role"].FirstOrDefault()
                        ?? context.Request.Query["asRole"].FirstOrDefault();

    // 2. ถ้าไม่มีใน Query ให้ตรวจใน Form Body (กรณี Submit Form เช่น UpdateStatus, ApprovePayment ฯลฯ)
    if (string.IsNullOrEmpty(roleParam) && context.Request.HasFormContentType)
    {
        try
        {
            var form = await context.Request.ReadFormAsync();
            roleParam = form["role"].FirstOrDefault()
                        ?? form["asRole"].FirstOrDefault();
        }
        catch { }
    }

    // 3. ถ้าไม่มี ให้ตรวจจาก Referer Header (แท็บเดิมที่กดลิงก์หรือส่งฟอร์ม)
    if (string.IsNullOrEmpty(roleParam))
    {
        string? referer = context.Request.Headers["Referer"].FirstOrDefault();
        if (!string.IsNullOrEmpty(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
        {
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(refererUri.Query);
            if (query.TryGetValue("role", out var rVal) || query.TryGetValue("asRole", out rVal))
            {
                roleParam = rVal.FirstOrDefault();
            }
            else
            {
                string refPath = refererUri.AbsolutePath.Trim().ToLowerInvariant();
                if (refPath.StartsWith("/owner")) roleParam = "Owner";
                else if (refPath.StartsWith("/admin")) roleParam = "Admin";
                else if (refPath.StartsWith("/inventory")) roleParam = "InventoryStaff";
                else if (refPath.StartsWith("/order")) roleParam = "OrderStaff";
                else if (refPath.StartsWith("/shipping")) roleParam = "ShippingStaff";
                else if (refPath.StartsWith("/marketing")) roleParam = "MarketingStaff";
                else if (refPath.StartsWith("/accounting")) roleParam = "AccountingStaff";
                else if (refPath.StartsWith("/support")) roleParam = "ITSupport";
            }
        }
    }

    // 4. ถ้ายังไม่มี แต่ Request ปัจจุบันกำลังเข้าถึงหน้า Back-Office ของพนักงาน/ผู้บริหาร
    // และสถานะ Cookie ปัจจุบันเป็น Customer (หรือ Guest) ซึ่งไม่มีสิทธิ์ใน Back-Office
    // ให้กำหนด Role ตามระบบงานที่กำลังเข้าถึงทันที เพื่อป้องกัน Access Denied ข้ามแท็บ
    string currentPath = context.Request.Path.Value?.Trim().ToLowerInvariant() ?? "";

    // หากเปิดหน้า Login หรือ Register (GET) ให้แสดงสถานะเป็น Guest เสมอ ไม่ดึง Cookie ของแท็บอื่นมาแสดงปน
    if ((currentPath.StartsWith("/auth/login") || currentPath.StartsWith("/auth/register")) &&
        HttpMethods.IsGet(context.Request.Method))
    {
        roleParam = "guest";
    }

    bool isBackOfficePath = currentPath.StartsWith("/inventory") ||
                            currentPath.StartsWith("/order") ||
                            currentPath.StartsWith("/shipping") ||
                            currentPath.StartsWith("/marketing") ||
                            currentPath.StartsWith("/accounting") ||
                            currentPath.StartsWith("/support") ||
                            currentPath.StartsWith("/admin") ||
                            currentPath.StartsWith("/owner");

    if (string.IsNullOrEmpty(roleParam) && isBackOfficePath)
    {
        bool isStaffOrAdminCookie = context.User?.Identity?.IsAuthenticated == true &&
                                    (context.User.IsInRole("Owner") ||
                                     context.User.IsInRole("Admin") ||
                                     context.User.IsInRole("InventoryStaff") ||
                                     context.User.IsInRole("OrderStaff") ||
                                     context.User.IsInRole("ShippingStaff") ||
                                     context.User.IsInRole("MarketingStaff") ||
                                     context.User.IsInRole("AccountingStaff") ||
                                     context.User.IsInRole("ITSupport"));

        if (!isStaffOrAdminCookie)
        {
            if (currentPath.StartsWith("/owner")) roleParam = "Owner";
            else if (currentPath.StartsWith("/admin")) roleParam = "Admin";
            else if (currentPath.StartsWith("/inventory")) roleParam = "InventoryStaff";
            else if (currentPath.StartsWith("/order")) roleParam = "OrderStaff";
            else if (currentPath.StartsWith("/shipping")) roleParam = "ShippingStaff";
            else if (currentPath.StartsWith("/marketing")) roleParam = "MarketingStaff";
            else if (currentPath.StartsWith("/accounting")) roleParam = "AccountingStaff";
            else if (currentPath.StartsWith("/support")) roleParam = "ITSupport";
        }
    }

    // 5. หากตรวจพบ Role/Username ของแท็บ ให้สร้าง ClaimsPrincipal จำลองสิทธิ์สำหรับ Request ของแท็บนี้ทันที (แยกแต่ละบัญชีออกจากกัน 100%)
    if (string.Equals(roleParam, "guest", StringComparison.OrdinalIgnoreCase))
    {
        context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity());
    }
    else if (!string.IsNullOrEmpty(roleParam))
    {
        var repo = context.RequestServices.GetRequiredService<PetShopRepository>();
        var (targetUsername, targetFullName, targetRole, targetUserId) = repo.ResolveTabUserInfo(roleParam);
        string? cookieUsername = context.User?.Identity?.Name;
        bool cookieMatchesExactUser = context.User?.Identity?.IsAuthenticated == true &&
                                      string.Equals(cookieUsername, targetUsername, StringComparison.OrdinalIgnoreCase);

        if (!cookieMatchesExactUser && !string.IsNullOrEmpty(targetUsername))
        {
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, targetUserId),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, targetUsername),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, targetRole),
                new System.Security.Claims.Claim("FullName", targetFullName),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, $"{targetUsername}@petshop.com"),
                new System.Security.Claims.Claim("Phone", "081-234-5678")
            };

            // หากเป็น Owner หรือ Admin ให้สิทธิ์เข้าถึงทุก Role
            if (targetRole == "Owner" || targetRole == "Admin")
            {
                foreach (UserRole r in Enum.GetValues(typeof(UserRole)))
                {
                    if (r.ToString() != targetRole)
                    {
                        claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, r.ToString()));
                    }
                }
            }

            var tabIdentity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            context.User = new System.Security.Claims.ClaimsPrincipal(tabIdentity);
        }
    }

    // 6. ตรวจสอบสถานะการระงับบัญชีในทุก Request: หากผู้ใช้ที่กำลังล็อกอินอยู่โดนระงับ ให้เตะออกจากระบบไปหน้า Login ทันที
    bool isExemptPath = currentPath.StartsWith("/auth/login") ||
                        currentPath.StartsWith("/auth/register") ||
                        currentPath.StartsWith("/auth/logout") ||
                        currentPath.StartsWith("/petshophub") ||
                        currentPath.StartsWith("/css") ||
                        currentPath.StartsWith("/js") ||
                        currentPath.StartsWith("/lib") ||
                        currentPath.StartsWith("/images") ||
                        currentPath.EndsWith(".html") ||
                        currentPath.EndsWith(".ico");

    if (!isExemptPath && context.User?.Identity?.IsAuthenticated == true)
    {
        string? activeUsername = context.User.Identity.Name;
        if (!string.IsNullOrEmpty(activeUsername))
        {
            var repo = context.RequestServices.GetRequiredService<PetShopRepository>();
            var dbUser = repo.FindUserByUsernameOrEmail(activeUsername);
            if (dbUser != null && !dbUser.IsActive)
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                string reason = Uri.EscapeDataString(dbUser.SuspensionReason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ");
                string until = Uri.EscapeDataString(dbUser.SuspensionDisplayUntil);
                string u = Uri.EscapeDataString(dbUser.Username);
                context.Response.Redirect($"/Auth/Login?suspended=true&u={u}&reason={reason}&until={until}");
                return;
            }
        }
    }

    await next();
});

app.UseAuthorization();

app.MapStaticAssets();

app.MapHub<CSI402_Project_final.Hubs.PetShopHub>("/petShopHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
