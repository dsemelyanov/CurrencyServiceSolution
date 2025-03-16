using BackgroundService.Services.HttpClients.CbrSender;
using BackgroundService.Services.Quartz;
using BackgroundService.Settings;
using Microsoft.EntityFrameworkCore;
using Quartz;
using SharedModels.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Регистрация кодировок
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<ICbrSender, CbrSender>();

// Привязка конфигурации к классу настроек
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// Регистрация Quartz
builder.Services.AddQuartz(q =>
{
    // Регистрация задачи
    var jobKey = new JobKey("CurrencyUpdateJob");
    q.AddJob<CurrencyUpdateJob>(opts => opts.WithIdentity(jobKey));

    // Настройка триггера (запуск каждые 24 часа)
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("CurrencyUpdateTrigger")
        .WithSimpleSchedule(x => x
            .WithIntervalInHours(24)
            .RepeatForever()));
});

// Использование Quartz как hosted service
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Использование стандартного BackgroundService
//builder.Services.AddHostedService<CurrencyUpdateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
