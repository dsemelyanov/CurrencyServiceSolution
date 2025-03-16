using FinanceService.Services.FinanceService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedModels.Data;
using SharedModels.Settings;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// �������� ������������
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// ����������� DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ����������� HttpClient
builder.Services.AddHttpClient();

// ����������� �������� JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// ����������� �������� Redis
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("RedisSettings"));

// ��������� �������������� JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully");
                return Task.CompletedTask;
            }
        };
    });

// ����������� Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisSettings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;
    return ConnectionMultiplexer.Connect(redisSettings.ConnectionString);
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IFinanceService, FinanceService.Services.FinanceService.FinanceService>();

//// ���������� �������� CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", builder =>
//    {
//        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
//    });
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//// ��������� ���������
//app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
