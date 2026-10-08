using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ProductHub.Application.Common.Abstractions;
using ProductHub.Infrastructure.Caching;
using ProductHub.Infrastructure.Identity;
using ProductHub.Infrastructure.Persistence;
using ProductHub.Infrastructure.Repositories;

namespace ProductHub.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Port=5432;Database=producthub_db;Username=postgres;Password=postgres";

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            var jwtSection = configuration.GetSection(JwtSettings.SectionName);
            services.Configure<JwtSettings>(jwtSection);

            var secretKey = jwtSection["SecretKey"]
                ?? configuration["JWT_SECRET"]
                ?? "SuperSecretKeyForDevelopmentTestingOnlyMustBeLongerThan32Bytes!";
            var issuer = jwtSection["Issuer"] ?? configuration["JWT_ISSUER"] ?? "ProductHub";
            var audience = jwtSection["Audience"] ?? configuration["JWT_AUDIENCE"] ?? "ProductHubUsers";

            var key = Encoding.UTF8.GetBytes(secretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            var redisConnectionString = configuration.GetConnectionString("Redis")
                ?? configuration["REDIS_CONNECTION"]
                ?? "localhost:6379";

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "ProductHub_";
            });

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<ICacheVersionService, CacheVersionService>();

            return services;
        }
    }
}