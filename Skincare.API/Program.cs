using Microsoft.EntityFrameworkCore;
using Skincare.API.Configurations;
using Skincare.API.Middleware;
using Skincare.Repositories.Context;
using Skincare.Services.Interfaces;
using Skincare.Services.Implements;
using Microsoft.AspNetCore.SignalR;
using Skincare.Services.Implements;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        options.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ssZ";
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Enhanced Swagger Configuration
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Skincare Store API",
        Version = "v1",
        Description = "Comprehensive API for Skincare Store e-commerce platform",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@skincare.com",
            Url = new Uri("https://skincare.com/support")
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "Skincare Store API",
        Version = "v2",
        Description = "Enhanced API with advanced features including search, analytics, and notifications",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@skincare.com",
            Url = new Uri("https://skincare.com/support")
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Add JWT Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });

    // Add examples
    options.ExampleFilters();

    // Add XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Add operation filters
    options.OperationFilter<AddFileParamOperationFilter>();
    options.OperationFilter<FileUploadOperationFilter>();

    // Add response examples
    options.ExampleFilters();

    // Group endpoints by version
    options.TagActionsBy(api =>
    {
        if (api.GroupName != null)
        {
            return new[] { api.GroupName.ToString() };
        }

        var controllerActionDescriptor = api.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        if (controllerActionDescriptor != null)
        {
            return new[] { controllerActionDescriptor.ControllerName };
        }

        throw new InvalidOperationException("Unable to determine tag for endpoint.");
    });

    options.DocInclusionPredicate((name, api) => true);
});

// Add Swagger Examples
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

// Add DbContext
builder.Services.AddDbContext<SWP391Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add SignalR for real-time notifications
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 102400; // 100KB
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

// Add custom services
builder.Services.AddServices();

// Add custom rate limiting
builder.Services.AddCustomRateLimiting(builder.Configuration);

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("Database")
    .AddCheck<RedisHealthCheck>("Redis")
    .AddCheck<SystemHealthCheck>("System")
    .AddCheck<ResponseTimeHealthCheck>("ResponseTime");

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/skincare-api-.log", rollingInterval: RollingInterval.Day));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Skincare Store API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Skincare Store API v2");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Skincare Store API Documentation";
        options.DefaultModelsExpandDepth(-1);
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.EnableFilter();
        options.ShowExtensions();
        options.ShowCommonExtensions();
        options.EnableValidator();
        options.EnableTryItOutByDefault();
        options.DisplayOperationId();
        options.InjectStylesheet("/swagger-ui/custom.css");
        options.InjectJavascript("/swagger-ui/custom.js");
    });
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowAll");

// Use custom middleware
app.UseCorrelationId();
app.UseGlobalExceptionHandler();
app.UseResponseTimeTracking();

// Use API standardization middleware
app.UseApiStandardization();

// Use rate limiting
app.UseRateLimiter();

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map health checks
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                description = x.Value.Description,
                duration = x.Value.Duration.ToString()
            })
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

// Map SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
