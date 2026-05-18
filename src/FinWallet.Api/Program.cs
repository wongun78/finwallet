using System.Text;
using FinWallet.Application;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Options;
using FinWallet.Infrastructure;
using FinWallet.Infrastructure.HealthChecks;
using FinWallet.Infrastructure.Persistence;
using FinWallet.Api.Middleware;
using FinWallet.Api.Security;
using FinWallet.Api.Services;
using FinWallet.Api.Support;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

builder.Configuration.AddEnvironmentVariables();

var corsOrigins = builder.Configuration.GetSection("Cors").GetValue<string>("AllowedOrigins") ?? string.Empty;
var allowedOrigins = corsOrigins
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

QuestPDF.Settings.License = LicenseType.Community;

builder.Host.UseSerilog((context, services, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services);

    var seq = context.Configuration.GetSection(SeqOptions.SectionName).Get<SeqOptions>();
    if (seq is not null && !string.IsNullOrWhiteSpace(seq.ServerUrl))
    {
        loggerConfig.WriteTo.Seq(seq.ServerUrl);
    }
});

builder.Services.AddControllers();
if (allowedOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", policy =>
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod());
    });
}
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "FinWallet API", Version = "v1" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhap JWT theo dinh dang: Bearer {token}"
    };

    options.AddSecurityDefinition("Bearer", scheme);

    var totpScheme = new OpenApiSecurityScheme
    {
        Name = "X-Totp-Code",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Ma TOTP 6 so tu app xac thuc"
    };

    options.AddSecurityDefinition("Totp", totpScheme);

    options.OperationFilter<AuthorizeOperationFilter>();
    options.OperationFilter<TotpHeaderOperationFilter>();
    options.OperationFilter<SwaggerExamplesOperationFilter>();
});

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(x => !string.IsNullOrWhiteSpace(x.Key), "Jwt:Key is required")
    .ValidateOnStart();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpClient();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<RedisHealthCheck>("redis")
    .AddCheck<RabbitMqHealthCheck>("rabbitmq")
    .AddCheck<MinioHealthCheck>("minio")
    .AddCheck<SeqHealthCheck>("seq");

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(app.Services);

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RateLimitMiddleware>();

if (allowedOrigins.Length > 0)
{
    app.UseCors("CorsPolicy");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
});

app.UseSwagger();
app.UseSwaggerUI();

app.Run();

public partial class Program
{
}
