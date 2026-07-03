using Immunitas.Application;
using Immunitas.Application.Services.CytometerDataProcessor;
using Immunitas.Application.Services.DataSeeding;
using Immunitas.Application.Services.Hashing;
using Immunitas.Application.Services.TokensGeneration;
using Immunitas.Application.Services.DaData;
using Immunitas.Domain.Repositories;
using Immunitas.Persistence;
using Immunitas.Persistence.Repositories;
using Immunitas.Portal.Bff.Consts;
using Immunitas.Portal.Bff.ExceptionHandlers;
using Immunitas.Portal.Bff.Extensions;
using Immunitas.Portal.Bff.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Python.Runtime;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

SeedDataConfig.SeedDataFilePath = Path.Combine(builder.Environment.ContentRootPath, "Data", "bloodtest_data.txt");


// Получаем строку подключения к БД
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Не найдена строка подключения к БД");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


// Получаем путь до библиотеки Python
var pythonDllPath = builder.Configuration["PYTHONNET_PYDLL"];
if (string.IsNullOrWhiteSpace(pythonDllPath))
    throw new InvalidOperationException("Не найден путь до pythonXX.dll");

Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDllPath);
PythonEngine.Initialize();
PythonEngine.BeginAllowThreads();


builder.WebHost.UseStaticWebAssets();

builder.Services.AddCommandAndQueryHandlers();
builder.Services.AddOpenApi();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IChangesSaver, ChangesSaver>();
builder.Services.AddControllers();

builder.Services.AddExceptionHandler<UserUnauthorizedExceptionHandler>();
builder.Services.AddExceptionHandler<EntityNotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddAntiforgery();

builder.Services.AddSingleton<ICytometerDataProcessor, CytometerDataProcessor>();
builder.Services.AddSingleton<IHashingService, BCryptHashingService>();
builder.Services.AddSingleton<IUserTokenService, UserTokenService>();

builder.Services.AddOptions<TokenCreationOptions>()
    .BindConfiguration(TokenCreationOptions.SectionName)
    .ValidateOnStart()
    .ValidateDataAnnotations();

builder.Services.AddScoped<UserContext>();

if (builder.Environment.IsDevelopment())
    builder.Services.AddSingleton<ISeedDataGenerator, TestSeedDataGenerator>();

var tokenCreationOptions = builder.Configuration.GetSection(TokenCreationOptions.SectionName).Get<TokenCreationOptions>()
    ?? throw new InvalidOperationException("Не найдены настройки генерации токенов");
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = tokenCreationOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = tokenCreationOptions.Audience,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenCreationOptions.Key)),
            ClockSkew = TimeSpan.Zero
        };
        options.MapInboundClaims = false;
    })
    .AddJwtBearer(AuthSchemas.RefreshTokenSchema, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = tokenCreationOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = tokenCreationOptions.Audience,
            ValidateLifetime = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenCreationOptions.Key)),
            ClockSkew = TimeSpan.Zero
        };
        options.MapInboundClaims = false;
    });

builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy(AuthPolicies.RefreshTokenPolicy, policy =>
    {
        policy.AuthenticationSchemes.Add(AuthSchemas.RefreshTokenSchema);
        policy.RequireAuthenticatedUser();
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWasm",
        policy =>
        {
            policy.WithOrigins("https://localhost:5054") // ����� Blazor WASM
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddHttpClient<IDaDataService, DaDataService>(client =>
{
    var daDataConfig = builder.Configuration.GetSection("DaData");
    var baseUrl = daDataConfig["BaseUrl"] ?? "https://suggestions.dadata.ru/";
    var apiKey = daDataConfig["ApiKey"] ?? throw new InvalidOperationException("Не найден ключ для сервиса DaData");

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Authorization", $"Token {apiKey}");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();
app.MigrateDatabase();

// OpenAPI/Swagger
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "v1");
});
app.UseReDoc(options =>
{
    options.SpecUrl("/openapi/v1.json");
});

app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ValidateSecurityStampMiddleware>();
app.UseMiddleware<UserContextMiddleware>();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

try
{
    PythonEngine.Shutdown();
}
catch
{
    // ignore
}