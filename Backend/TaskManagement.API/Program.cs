using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.Mappings;
using TaskManagement.API.Services.Interfaces;
using TaskManagement.API.Services.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskManagement.API.Services;
using TaskManagement.API.Middleware;
using TaskManagement.API.Responses;
using System.Security.Claims;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/startup-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14)
    .CreateBootstrapLogger();
try {
    Log.Information("Uygulama başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();
    });

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "Task Management API",
            Version = "v1"
        });

        // JWT desteği
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Bearer {token} formatında JWT giriniz."
        });

        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services
        .AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var message = context.ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage)
                    .FirstOrDefault(error => !string.IsNullOrWhiteSpace(error))
                    ?? "Gönderilen bilgiler geçersiz.";

                return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = message,
                        Data = null
                    });
            };
        });

    builder.Services.AddAutoMapper(typeof(MappingProfile));

    builder.Services.AddScoped<IUserService, UserService>();

    builder.Services.AddScoped<ITaskService, TaskService>();

    builder.Services.AddScoped<ITaskAttachmentService, TaskAttachmentService>();

    builder.Services.AddScoped<ITaskCommentService, TaskCommentService>();

    builder.Services.AddScoped<ICategoryService, CategoryService>();

    builder.Services.AddScoped<IJwtService, JwtService>();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddScoped<IAdminSeedService, AdminSeedService>();

    var jwtSettings = builder.Configuration.GetSection("Jwt");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                    Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),

                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role,

                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    var databaseProvider = builder.Configuration["DatabaseProvider"]?.Trim();

    var healthChecks = builder.Services.AddHealthChecks();

    if (string.Equals(databaseProvider, "PostgreSql", StringComparison.OrdinalIgnoreCase))
    {
        healthChecks.AddNpgSql(
            builder.Configuration.GetConnectionString("PostgreSql")!,
            name: "PostgreSQL");
    }

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        if (string.Equals(databaseProvider, "Oracle", StringComparison.OrdinalIgnoreCase))
        {
            options.UseOracle(
                builder.Configuration.GetConnectionString("Oracle"),
                provider => provider
                    .MigrationsAssembly("TaskManagement.API.Migrations.Oracle")
                    .MigrationsHistoryTable("__EFMigrationsHistory"));
        }
        else if (string.Equals(
                    databaseProvider,
                    "PostgreSql",
                    StringComparison.OrdinalIgnoreCase))
        {
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("PostgreSql"),
                provider => provider
                    .MigrationsAssembly("TaskManagement.API.Migrations.PostgreSql")
                    .MigrationsHistoryTable("__EFMigrationsHistory"));
        }
        else
        {
            throw new InvalidOperationException(
                "DatabaseProvider değeri 'PostgreSql' veya 'Oracle' olmalıdır.");
        }
    });
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngular", policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        await using var scope = app.Services.CreateAsyncScope();
        var adminSeedService = scope.ServiceProvider.GetRequiredService<IAdminSeedService>();
        await adminSeedService.SeedAsync();

        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseCors("AllowAngular");

    app.UseMiddleware<ExceptionMiddleware>();

    app.UseSerilogRequestLogging();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    app.MapHealthChecks("/health");

    app.Run();
}

catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama beklenmeyen bir şekilde sonlandı.");
}

finally
{
    Log.CloseAndFlush();
}