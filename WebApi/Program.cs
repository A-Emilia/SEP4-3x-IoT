using System.Text.Json.Serialization;
using MongoDB.Driver;
using Repositories;
using Repositories.PostgreSQL;
using RepositoryContracts;
using WebApi.TCP;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");

var jwtKey = jwtSettings["Key"]
    ?? throw new InvalidOperationException("JWT Key is missing.");

var key = Encoding.UTF8.GetBytes(jwtKey);

var issuer = jwtSettings["Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer is missing.");

var audience = jwtSettings["Audience"]
    ?? throw new InvalidOperationException("JWT Audience is missing.");

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

            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

string mongoConnectionString = builder.Configuration["AZURE_COSMOS_CONNECTIONSTRING"]
    ?? throw new InvalidOperationException("Missing Azure Cosmos connection string.");

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(mongoConnectionString));

builder.Services.AddSingleton(sp => {
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("measurement_data");
});



builder.Services.AddScoped<IMeasurementRepository, MeasurementRepository>();
/*
 * I need to dependency inject these.
 */

string postgresConnectionString = builder.Configuration.GetConnectionString("Postgres")
        ?? throw new InvalidOperationException("Missing Postgres connection string.");

builder.Services.AddScoped<IUserRepository>(_ => new UserRepository(postgresConnectionString));
builder.Services.AddScoped<IRoomRepository>(_ => new RoomRepository(postgresConnectionString));
builder.Services.AddScoped<IDeviceRepository>(_ => new DeviceRepository(postgresConnectionString));
builder.Services.AddScoped<IDeviceActionLogRepository>(_ =>
    new DeviceActionLogRepository(postgresConnectionString));

builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddHostedService<TCPService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
}
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();