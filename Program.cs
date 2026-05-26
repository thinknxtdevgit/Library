using lib.Dbcontext;
using lib.Interface;
using lib.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// CONNECTION STRING
// =====================================================

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection");

// =====================================================
// DB CONTEXT
// =====================================================

builder.Services.AddDbContext<AppDbcontext>(options =>
    options.UseSqlServer(connectionString));

// =====================================================
// SERVICES
// =====================================================

builder.Services.AddScoped<ILoginService, LoginService>();

builder.Services.AddScoped<IIssueBookService, IssueBookService>();

builder.Services.AddScoped<IReturnBookService, ReturnBookService>();

builder.Services.AddScoped<IRenewBookService, RenewBookService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IStockBookService, StockBookService>();


// =====================================================
// SESSION
// =====================================================
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();



// =====================================================
// MVC
// =====================================================

builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();

// =====================================================
// BUILD
// =====================================================

var app = builder.Build();

// =====================================================
// ERROR HANDLING
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// =====================================================
// MIDDLEWARE
// =====================================================

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// ✅ Session MUST come after Routing and before Authorization
app.UseSession();

app.UseAuthorization();

// MVC routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.MapControllers();

app.Run();