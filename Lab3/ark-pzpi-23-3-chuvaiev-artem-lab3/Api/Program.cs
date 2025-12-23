using Api.Infrastructure;
using Api.Infrastructure.Data;
using Api.Infrastructure.Repository;
using Api.Models;
using Api.Services;
using Api.Services.Interfaces;
using Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;
using System.Globalization;
using Log = Serilog.Log;

var builder = WebApplication.CreateBuilder(args);

// Clear default logging providers and configure Serilog
builder.Logging.ClearProviders();

// Configure Serilog as the logging provider
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Configure JSON options to handle circular references
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddDbContext<ApplicationDbContext>(opts =>
{
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DataContext"));
});
Console.Write(builder.Configuration.GetConnectionString("DataContext"));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register Repositories
builder.Services.AddScoped<IGenericRepository<User>, GenericRepository<User>>();
builder.Services.AddScoped<IGenericRepository<Role>, GenericRepository<Role>>();
builder.Services.AddScoped<IGenericRepository<UserRole>, GenericRepository<UserRole>>();
builder.Services.AddScoped<IGenericRepository<Package>, GenericRepository<Package>>();
builder.Services.AddScoped<IGenericRepository<DeliveryStatus>, GenericRepository<DeliveryStatus>>();
builder.Services.AddScoped<IGenericRepository<PackageCategory>, GenericRepository<PackageCategory>>();

// Register Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INfcService, NfcService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();

// Lab 3: Register Analytics and Admin Services
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Add Swagger for API documentation (optional but useful)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Seed database if PackageCategories table is empty
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var seeder = new DatabaseSeeder(context);
        await seeder.SeedAsync();
        Log.Information("Database seeding completed");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while seeding the database");
    }
}

// Lab 3: Configure Localization (support for Ukrainian and English)
var supportedCultures = new[]
{
    new CultureInfo("en-US"),
    new CultureInfo("uk-UA")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Exception handler should be early in the pipeline
app.UseExceptionHandler();

// Request logging should be after exception handler to log errors
app.UseSerilogRequestLogging();

// Configure middleware pipeline

app.UseSwagger();
app.UseSwaggerUI();


app.UseRouting();

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.MapControllers();

try
{
    Log.Information("Application started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}