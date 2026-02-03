using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository.Data;
using Repository.Implement;
using Repository.Interface;
using Service.Implement;
using Service.Interface;
using Service.Interfaces;
using Service.Services;
using StudyMate_BE.BackgroundServices;

namespace StudyMate_BE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===================== DB CONTEXT =====================
            builder.Services.AddDbContext<StudyMateContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), null);
                    }));

            // ===================== DEPENDENCY INJECTION =====================
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            builder.Services.AddScoped<ICourse2Service, Course2Service>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();


            builder.Services.AddHostedService<AdminSeederService>();
            builder.Services.AddHostedService<TokenCleanupService>();

            // ===================== JWT AUTH =====================
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(secretKey!)
                        ),
                        ClockSkew = TimeSpan.Zero
                    };

                    // Custom event to accept token with or without "Bearer" prefix
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var authorizationHeader = context.Request.Headers["Authorization"].ToString();
                            
                            if (!string.IsNullOrEmpty(authorizationHeader))
                            {
                                // If token starts with "Bearer ", remove it
                                if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                                {
                                    context.Token = authorizationHeader.Substring("Bearer ".Length).Trim();
                                }
                                // If it's just the token without "Bearer", use it directly
                                else
                                {
                                    context.Token = authorizationHeader.Trim();
                                }
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization();

            // ===================== CORS =====================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // ===================== CONTROLLERS =====================
            builder.Services.AddControllers();

            // ===================== SWAGGER + JWT =====================
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "StudyMate API",
                    Version = "v1"
                });

                // 🔐 JWT Bearer
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter: Bearer {your JWT token}"
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

            // ===================== AUTO MIGRATION =====================
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StudyMateContext>();
                context.Database.Migrate();
            }

            // ===================== GLOBAL EXCEPTION =====================
            app.Use(async (context, next) =>
            {
                try { await next(); }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "Internal Server Error",
                        error = ex.Message
                    });
                }
            });

            // ===================== MIDDLEWARE =====================
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "StudyMate API V1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseCors("AllowAll");

            app.UseAuthentication(); // 🔥 PHẢI TRƯỚC
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}
