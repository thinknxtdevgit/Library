using lib.Dbcontext;
using lib.Interface;
using lib.Service;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// 1. Connection String read karein
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. DB Context (EF Core ke liye)
builder.Services.AddDbContext<AppDbcontext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IIssueBookService,IssueBookService>();
builder.Services.AddScoped<IReturnBookService, ReturnBookService>();
builder.Services.AddScoped<IRenewBookService, RenewBookService>();

// 3. MVC + API Support
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

//Session
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();



var app = builder.Build();
app.UseSession();
// --- Middleware Pipeline ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // CSS/JS files ke liye zaruri hai
app.UseRouting();

app.UseAuthorization();

// 1. API Routing (Ye [Route("api/...")] wale actions ko enable karega)
app.MapControllers();

// 2. Default View Routing (localhost:5065 kholne par Admissions page dikhayega)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();