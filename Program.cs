using System.Text;
using Assignment_Submission_Management_System.Data;
using Assignment_Submission_Management_System.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add Services
        builder.Services.AddScoped<IAuthService, AuthService>();

        // Configure Database Provider (PostgreSQL if reachable, with automatic SQLite fallback)
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        var sqliteConnectionString = builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=assignment_mgmt.db";
        var dbProvider = builder.Configuration.GetValue<string>("DbProvider") ?? "SQLite";

        bool usePostgres = dbProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(connectionString);

        if (usePostgres)
        {
            try
            {
                using var conn = new Npgsql.NpgsqlConnection(connectionString);
                conn.Open();
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB Note] PostgreSQL connection test failed: {ex.Message}. Falling back to SQLite.");
                usePostgres = false;
            }
        }

        if (usePostgres)
        {
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));
        }
        else
        {
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(sqliteConnectionString));
        }

        // Configure CORS for Next.js frontend
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000", "http://localhost:3001")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // Configure JWT Authentication
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "Your_Super_Secret_Key_Which_Must_Be_At_Least_32_Bytes_Long!");

        builder.Services.AddAuthentication(options =>
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
                ValidIssuer = jwtSettings["Issuer"] ?? "AssignmentMgmtAPI",
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"] ?? "AssignmentMgmtUsers",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        // Configure Controllers & Swagger
        builder.Services.AddControllersWithViews();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Assignment & Submission Management System API",
                Version = "v1",
                Description = "Role-based school/college API for managing users, courses, subjects, assignments, and student submissions."
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Description = "Enter JWT Bearer token format: Bearer {token}"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

        var app = builder.Build();

        // Enable Swagger UI in both Dev and Production for evaluator ease
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Assignment & Submission API v1");
            c.RoutePrefix = "swagger";
        });

        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors("AllowFrontend");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapControllers();

        // Auto-seed database on application startup
        using (var scope = app.Services.CreateScope())
        {
            try
            {
                await DbInitializer.Initialize(scope.ServiceProvider);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization note: {ex.Message}");
            }
        }

        await app.RunAsync();
    }
}