using BusinessLayer.Interface;
using BusinessLayer.Service;
using EmailService.Interface;
using EmailService.Model;
using EmailService.Service;
using Fundoo.Middleware;         // NEW
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using RepositoryLayer.Context;
using RepositoryLayer.Interface;
using RepositoryLayer.Service;
using System.Reflection;
using System.Text;

namespace Fundoo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Host.UseNLog();

            builder.Services.AddControllers();

            // NEW: global exception handling
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Fundoo Notes API",
                        Version = "v1",
                        Description = "API for Fundoo Notes user registration, login, password reset and profile access."
                    });

                string xmlFile =
                    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

                string xmlPath =
                    Path.Combine(AppContext.BaseDirectory, xmlFile);

                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                options.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter JWT token"
                    });

                options.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
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

            builder.Services.AddScoped<IUserBL, UserBL>();

            builder.Services.AddScoped<IUserRL, UserRL>();

            builder.Services.AddScoped<INoteBL, NoteBL>();

            builder.Services.AddScoped<INoteRL, NoteRL>();

            builder.Services.AddDbContext<FundooContext>(
                options =>
                {
                    options.UseNpgsql(
                        builder.Configuration.GetConnectionString("FundooConnection"));
                });

            EmailSettings emailSettings = new EmailSettings
            {
                Email = builder.Configuration["EmailSettings:Email"]!,
                Password = builder.Configuration["EmailSettings:Password"]!,
                Host = builder.Configuration["EmailSettings:Host"] ?? "smtp.gmail.com",
                Port = int.Parse(
                    builder.Configuration["EmailSettings:Port"] ?? "587")
            };

            builder.Services.AddSingleton(emailSettings);

            builder.Services.AddScoped<IEmailService, EmailService.Service.EmailService>();

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

            builder.Services.AddAuthorization(options =>
            {
                options.DefaultPolicy =
                    new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .RequireClaim("token_type", "access")
                        .Build();

                options.AddPolicy("ResetPasswordPolicy", policy =>
                {
                    policy.RequireAuthenticatedUser()
                          .RequireClaim("token_type", "password_reset");
                });
            });

            var app = builder.Build();

            // NEW: must come first so it catches errors from everything below
            app.UseExceptionHandler();

            app.UseSwagger();

            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseCors("AllowFrontend");

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }

}