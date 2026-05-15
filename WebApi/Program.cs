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
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

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

var connectionString =
    "mongodb://mongodb:mongodb@localhost:27018/measurement_data?authSource=admin";

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(connectionString));

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

builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddHostedService<TCPService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

app.Run();