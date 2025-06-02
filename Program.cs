using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TelegramWebAPI.Data;
using TelegramWebAPI.Hubs;
using TelegramWebAPI.Models;
using TelegramWebAPI.Services;
using TelegramWebAPI.Services.Interfaces;
using TelegramWebAPI.Settings;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers + FluentValidation
builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());

// 2. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TelegramWebAPI", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Введите JWT токен как: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference
                { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// 3. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b =>
    {
        b
        .AllowAnyOrigin() 
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); // важно для SignalR
    });
});

// 4. Authentication: JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var config = builder.Configuration;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = config["Jwt:Issuer"],
        ValidAudience = config["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
    };
});

// 5. Entity Framework (SQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 6. Cosmos DB
builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config["Azure:AzureCosmosDb:ConnectionString"];
    return new CosmosClient(connectionString);
});

builder.Services.AddSingleton<CosmosDbService>();
builder.Services.AddSingleton<IChatService, ChatService>();

// 7. Azure Blob Storage
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

// 8. SignalR
builder.Services.AddSignalR();
builder.Services.AddScoped<SignalRService>();

// 9. Пользовательские сервисы
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
builder.Services.AddScoped<IPasswordHasher<TelegramWebAPI.Models.User>, PasswordHasher<TelegramWebAPI.Models.User>>();

// 10. Конфигурация настроек
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<AzureBlobStorageSettings>(builder.Configuration.GetSection("Azure:AzureStorage"));
builder.Services.Configure<AzureCosmosDbSettings>(builder.Configuration.GetSection("Azure:AzureCosmosDb"));
builder.Services.Configure<SignalRSettings>(builder.Configuration.GetSection("Azure:SignalR"));

// 11. Валидация
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Middleware
// 1. Swagger (можно оставить как есть)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 2. Статические файлы (если используешь wwwroot)
app.UseStaticFiles();

// 3. Routing обязательно до CORS, Auth и MapHub
app.UseRouting();

// 4. CORS должен быть ДО Auth и Authorization
app.UseCors("AllowAll");

// 5. Аутентификация и авторизация
app.UseAuthentication();
app.UseAuthorization();

// 6. Endpoints
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chatHub"); // важно
});

app.Run();