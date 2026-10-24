
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Email;
using TaskManager.Infrastructure.Identity;
using TaskManager.Infrastructure.Repositories;
using TaskManager.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Common services
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddDataProtection();

// ASP.NET Core Identity
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(5);
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Gmail SMTP configuration
builder.Services.Configure<GmailSmtpOptions>(
    builder.Configuration.GetSection("GmailSmtp"));

builder.Services.AddScoped<IEmailService, GmailEmailService>();

// Email verification configuration
builder.Services
    .AddOptions<EmailVerificationOptions>()
    .Bind(builder.Configuration.GetSection("EmailVerification"))
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(options.HmacKey) &&
            options.HmacKey.Length == 64 &&
            options.HmacKey.All(Uri.IsHexDigit),
        "EmailVerification:HmacKey must be a 32-byte hexadecimal key.")
    .Validate(
        options => options.CodeLifetimeMinutes > 0,
        "Email verification code lifetime must be greater than zero.")
    .ValidateOnStart();

builder.Services.AddScoped<
    IEmailVerificationService,
    EmailVerificationService>();

// JWT configuration
builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(options.Key) &&
            Encoding.UTF8.GetByteCount(options.Key) >= 32,
        "JWT key must be at least 32 bytes.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Issuer),
        "JWT issuer is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Audience),
        "JWT audience is required.")
    .Validate(
        options => options.AccessTokenLifetimeMinutes > 0,
        "JWT lifetime must be greater than zero.")
    .ValidateOnStart();

// JWT Bearer authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration
            .GetSection("Jwt")
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "JWT configuration is missing.");

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.Key)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
    });

builder.Services.AddAuthorization();

// Application services and repositories
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// CORS for React/Vite frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Request logging
app.Use(async (context, next) =>
{
    Console.WriteLine(
        $"--> {context.Request.Method} " +
        $"{context.Request.Path}{context.Request.QueryString}");

    await next();

    Console.WriteLine($"<-- {context.Response.StatusCode}");
});

// HTTP request pipeline
app.UseCors("ReactFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
