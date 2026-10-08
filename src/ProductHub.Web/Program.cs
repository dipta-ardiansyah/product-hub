using Microsoft.OpenApi.Models;
using ProductHub.Application;
using ProductHub.Infrastructure;
using ProductHub.Infrastructure.Persistence;
using ProductHub.Web.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// 2. Add Layer Services
builder.Services.AddApplication();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 3. Exception Handler & ProblemDetails
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 4. Controllers & Views
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

// 5. Swagger with Bearer Auth
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ProductHub API",
        Version = "v1",
        Description = "Clean Architecture CQRS Web API for Product Management"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token only (without Bearer prefix in Swagger UI)"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 6. Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

var app = builder.Build();

// 7. Auto-migrate database in Development / non-testing environment
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    using var scope = app.Services.CreateScope();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Could not automatically initialize database. Ensure PostgreSQL is running.");
    }
}

// 8. Middleware Pipeline
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductHub API v1");
    });
}
else
{
    app.UseHsts();
}

app.UseSerilogRequestLogging();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

try
{
    Log.Information("Starting ProductHub Web Application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ProductHub application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
