using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Middleware.ExceptionHandling;
using Middleware.Services.Cache;
using Middleware.Services.Email;
using Middleware.Services.Jwt;
using RepositoryLayer.Context;
using RepositoryLayer.Interface;
using RepositoryLayer.Service;
using System.Text;

namespace Fundoo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load email credentials from secrets.json
            builder.Configuration.AddJsonFile(
                "secrets.json",
                optional: true,
                reloadOnChange: true);

            // Controllers
            builder.Services.AddControllers();

            // Global exception handling
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            // Swagger
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Fundoo Notes API",
                    Version = "v1"
                });

                c.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter JWT token"
                    });

                // Authorize ka token har request ke saath bhejta hai
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
                        new string[] { }
                    }
                });
            });

            // Dependency Injection
            builder.Services.AddScoped<IUserBL, UserBL>();
            builder.Services.AddScoped<IUserRL, UserRL>();
            builder.Services.AddScoped<INoteBL, NoteBL>();
            builder.Services.AddScoped<INoteRL, NoteRL>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IJwtService, JwtService>();

            // Caching (Memurai / Redis)
            builder.Services.AddAppCaching(builder.Configuration);

            // PostgreSQL
            builder.Services.AddDbContext<FundooContext>(options =>
            {
                options.UseNpgsql(
                    builder.Configuration
                        .GetConnectionString("FundooConnection"));
            });

            // JWT Authentication
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidIssuer =
                                builder.Configuration["Jwt:Issuer"],

                            ValidAudience =
                                builder.Configuration["Jwt:Audience"],

                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        builder.Configuration["Jwt:Key"]!))
                        };
                });

            var app = builder.Build();

            // Global exception handler
            app.UseExceptionHandler();

            // Swagger
            app.UseSwagger();
            app.UseSwaggerUI();

            // HTTPS
            app.UseHttpsRedirection();

            // Authentication
            app.UseAuthentication();

            // Authorization
            app.UseAuthorization();

            // Response caching middleware (auth ke BAAD, controllers ke PEHLE)
            app.UseAppCaching();

            // Controllers
            app.MapControllers();

            app.Run();
        }
    }
}