using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using originalstoremada.Data;
// using originalstoremada.Services.BackgroundService;
using originalstoremada.Services.Mail;
using Rotativa.AspNetCore;
using TimeZoneConverter;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

var smtpConfig = builder.Configuration.GetSection("SmtpConfig").Get<SmtpConfig>();
builder.Services.AddSingleton(smtpConfig);

// builder.Services.AddScoped<CheckBoutique>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddMvc().AddSessionStateTempDataProvider();

builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddHttpClient();
builder.Services.AddLogging(builder =>
{
    // builder.AddConsole();
});
// builder.Services.AddHostedService<CoursEuroBack>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Use localization middleware to set the culture for each request
var supportedCultures = new[] { new CultureInfo("fr-MG") };
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("fr-MG"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

app.UseRequestLocalization(localizationOptions);

//------------------------------

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

// sudo apt-get update
// sudo apt-get install wkhtmltopdf
// wkhtmltopdf --version

RotativaConfiguration.Setup(((IHostingEnvironment)app.Environment).ToString(), "/usr/bin/");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Client}/{action=Home}/{id?}");
app.MapRazorPages();

app.Run();