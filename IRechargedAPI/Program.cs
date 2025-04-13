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

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));

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
    options.AddPolicy("AllowAll", builder =>
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

builder.Services.AddHealthChecksUI(options =>
{
    options.AddHealthCheckEndpoint("API", "/health");
    options.AddHealthCheckEndpoint("Main DB", "/health/maindb");
    options.AddHealthCheckEndpoint("Auth DB", "/health/authdb");
    options.SetEvaluationTimeInSeconds(30);
})
.AddSqlServerStorage(builder.Configuration.GetConnectionString("HealthChecksUI"));

// Identity
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
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
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Health Checks endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/maindb", new HealthCheckOptions
{
    Predicate = reg => reg.Tags.Contains("database"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/authdb", new HealthCheckOptions
{
    Predicate = reg => reg.Tags.Contains("auth"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
    options.ApiPath = "/health-api";
});

app.MapControllers();

app.Run();