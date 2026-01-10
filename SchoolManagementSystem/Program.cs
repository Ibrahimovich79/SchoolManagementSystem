using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<SchoolDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Add Identity (With Role Support and UI)
// REMOVED: The conflicting "AddDefaultIdentity" line was deleted from here.

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
    // Password settings
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<SchoolDbContext>()
.AddDefaultUI()
.AddDefaultTokenProviders();

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddSingleton<AttendanceReportService>();
builder.Services.AddSingleton<IAttendanceReportService>(sp => sp.GetRequiredService<AttendanceReportService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<AttendanceReportService>());

builder.Services.AddRazorPages(options =>
{
    // Secure the Admin folder by default
    options.Conventions.AuthorizeFolder("/Admin", "RequireAdminRole");
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin", "Supervisor"));
});

var app = builder.Build();

// 3. Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// 4. Seed Database (Roles & Admin User)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

app.Run();