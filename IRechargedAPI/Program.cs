using IRecharge_API.BLL;
using IRecharge_API.DAL;
using IRecharge_API.ExternalServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using IRecharge_API.BLL.AuthService;
using IRecharge_API.MapConfig;
using Serilog;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using Serilog.Events;
using Azure.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger Authorization UI
builder.Services.AddSwaggerGen(Options =>
{
    Options.SwaggerDoc("v1", new() { Title = "SchoolManagementAPI", Version = "v1" });
    Options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
    });

    Options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme,
                },

                Scheme = "OAuth2",
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header
            },
            //Array.Empty<string>()
            new List<string> ()
        }
});
});

// DbContext
var connectionstrings = builder.Configuration.GetConnectionString("IRechargeDb");
builder.Services.AddDbContext<IRechargeDbContext>(options =>
{
    options.UseSqlServer(connectionstrings, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MapConfig).Assembly);

// IRechargeAuthDB
builder.Services.AddDbContext<IRechargeAuthDB>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IRechargeAuthDB")));

// Register HttpClient
builder.Services.AddHttpClient("DigitalVendorsUrl", client =>
{
    client.BaseAddress = new Uri("https://api3.digitalvendorz.com/api/");
});

// Serilog configuration with comprehensive health check suppression
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration)
    // 👇 Add just this one line to block health check logs
    .MinimumLevel.Override("Microsoft.AspNetCore.Diagnostics.HealthChecks", LogEventLevel.Fatal)
);


// Services
builder.Services.AddTransient<IPurchaseService, PurchaseService>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IDigitalVendors, DigitalVendorsAPI>();
builder.Services.AddScoped<IAuthManager, AuthManager>();
builder.Services.AddScoped<AirtimeService>();
builder.Services.AddSingleton<TokenServices>();
builder.Services.AddMemoryCache();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("HealthChecks", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("IRechargeDb"),
        name: "main-db",
        healthQuery: "SELECT 1;",
        tags: new[] { "database" }
    )
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("IRechargeAuthDB"),
        name: "auth-db",
        healthQuery: "SELECT 1;",
        tags: new[] { "database", "auth" }
    );


// Identity
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 9;
    options.Password.RequiredUniqueChars = 2;
    //options.User.RequireUniqueEmail = true;
    //options.SignIn.RequireConfirmedAccount = false;
    //options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    //options.Lockout.AllowedForNewUsers = true;
    //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<IRechargeAuthDB>()
    .AddDefaultTokenProviders();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSerilogRequestLogging(options =>
{
    // Add this check to exclude health endpoints
    options.GetLevel = (ctx, _, ex) =>
        ctx.Request.Path.StartsWithSegments("/health") ?
            LogEventLevel.Fatal : // Will NOT log
            (ex != null ? LogEventLevel.Error : LogEventLevel.Information);
});


app.UseCors("HealthChecks");
app.UseAuthentication();
app.UseAuthorization();


// Helps to reduce console noise for health Checks

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    AllowCachingResponses = true // Reduce processing
});

app.MapHealthChecks("/health/maindb", new HealthCheckOptions
{
    Predicate = reg => reg.Tags.Contains("database"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    AllowCachingResponses = true
});

app.MapHealthChecks("/health/authdb", new HealthCheckOptions
{
    Predicate = reg => reg.Tags.Contains("auth"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    AllowCachingResponses = true
});
app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
    options.ApiPath = "/health-api";
});

app.MapControllers();

app.Run();