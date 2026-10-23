using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.Data;
using TaskManager.Application.Interfaces;
using TaskManager.Infrastructure.Repositories;
using TaskManager.Application.Services;
using Microsoft.AspNetCore.Identity;
using TaskManager.Infrastructure.Identity;
using TaskManager.Infrastructure.Services;
using TaskManager.Infrastructure.Email;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<TimeProvider>(
    TimeProvider.System);

builder.Services.AddDataProtection();


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

builder.Services.Configure<GmailSmtpOptions>(
    builder.Configuration.GetSection("GmailSmtp"));

builder.Services
    .AddOptions<EmailVerificationOptions>()
    .Bind(builder.Configuration.GetSection("EmailVerification"))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.HmacKey),
        "EmailVerification:HmacKey is required.")
    .ValidateOnStart();


builder.Services.AddScoped<IEmailVerificationService,EmailVerificationService>();
builder.Services.AddScoped<IEmailService, GmailEmailService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IAuthService, AuthService>();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    Console.WriteLine(
        $"--> {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");

    await next();

    Console.WriteLine(
        $"<-- {context.Response.StatusCode}");
});

app.UseCors("ReactFrontend");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


if (app.Environment.IsDevelopment())
{
    app.MapPost("/api/dev/test-email", async (
        string to,
        IEmailService emailService,
        CancellationToken cancellationToken) =>
    {
        await emailService.SendEmailAsync(
            to,
            "Task Manager SMTP Test",
            "Your Gmail SMTP configuration is working.",
            cancellationToken);

        return Results.Ok(new
        {
            message = "Test email sent successfully."
        });
    });
}

app.Run();
