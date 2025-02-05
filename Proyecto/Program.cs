
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using proyecto.Contracts;
using proyecto.Data;
using proyecto.Repositories;
using proyecto.Helpers;
using proyecto.Models.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using proyecto.Services;

// Create Database if not exists
using (var db = new AnalysisDbContext())
{
    db.Database.EnsureCreated();
    db.SaveChanges();
}


var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5000");



builder.Services.AddScoped<IFilterFPY, FiltersFPY>();

var semaphore = new SemaphoreSlim(1, 1); // Inicializa el semáforo

builder.Services.AddSingleton(semaphore); // Registra el semáforo como singleton
builder.Services.AddHostedService<HourlyHostedService>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<HourlyHostedService>>();
    var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>(); // Cambia esta línea
    return new HourlyHostedService(logger, scopeFactory, semaphore);
});



builder.Services.AddSingleton<IMesRepository, MesRepository>();
builder.Services.AddSingleton<IMesRepositoryAguascalientes, MesRepositoryAguacalientes>();
builder.Services.AddScoped<IMesRepositoryFPY, MesRepositoryFPY>();
builder.Services.AddScoped<IDbContextWip, LineWorksWipRepository>();
builder.Services.AddDbContext<AnalysisDbContext>();
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddScoped<IFilterDiagStock, FilterDiagStock>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();



// Swagger config for authentication.
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});

// Authentication settings
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateActor = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization();


// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("corsconfig", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("corsconfig");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();