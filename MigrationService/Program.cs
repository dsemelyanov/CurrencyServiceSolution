using Microsoft.EntityFrameworkCore;
using SharedModels.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// �������� ������������
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

//// ���������� �������� ��� ������
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    Console.WriteLine("Applying migrations...");
//    await dbContext.Database.MigrateAsync();
//    Console.WriteLine("Migrations applied successfully.");
//}

//// ���������� ������ (����� ������������ ��� ������� ��� ������)
//Console.WriteLine("Migration service completed. Exiting...");
//Environment.Exit(0); // ��������� �������

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
