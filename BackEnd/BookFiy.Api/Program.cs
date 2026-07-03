using BookFiy.Api.Middlewares;
using BookFiy.Application.Interfaces;
using BookFiy.Application.Interfaces;
using BookFiy.Application.Services;
using BookFiy.Application.Services;
using BookFiy.Application.Validtors;
using BookFiy.Domain.Entites;
using BookFiy.Domain.Entities;
using BookFiy.Domain.IRepositories;
using BookFiy.Infrastructure.Data.Context;
using BookFiy.Infrastructure.Data.SeedData;
using BookFiy.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// bind SmtpSettings
builder.Services.Configure<BookFiy.Application.Settings.SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;

        options.User.RequireUniqueEmail = true;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
// jwt 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})  
    .AddJwtBearer
    (
        "JwtBearer",
        options =>
        {
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:issuer"],
                ValidAudience = builder.Configuration["Jwt:audience"],
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:key"]!)
                ),
            };
            // Add this event handler
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"Auth failed: {context.Exception.GetType().Name}: {context.Exception.Message}");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine("Token validated successfully!");
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    Console.WriteLine($"Challenge: {context.Error} - {context.ErrorDescription}");
                    return Task.CompletedTask;
                }
            };
        }
    );

builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>();
// register validators explicitly (assembly scanning already handles this but ensure presence)
builder.Services.AddValidatorsFromAssemblyContaining<BookFiy.Application.Validtors.RegisterConfirmValidator>();


// register services with DI 
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IEmailService, EmailService>();
// service domain
builder.Services.AddScoped<BookFiy.Domain.IRepositories.IServiceRepository, BookFiy.Infrastructure.Repositories.ServiceRepository>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<BookFiy.Application.Interfaces.ITenantProvider, BookFiy.Application.Services.TenantProvider>();
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingStatusRepository, BookingStatusRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<RedisService>();
// refresh token repository
builder.Services.AddScoped<BookFiy.Domain.IRepositories.IRefreshTokenRepository, BookFiy.Infrastructure.Repositories.RefreshTokenRepository>();

// redis
var redis = ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false");
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();

app.MapControllers();

// Seed
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();