
using yazlab1proje3webapi.Classes;
using yazlab1proje3webapi.Models.Context;
using yazlab1proje3webapi.Repositories.AdminRepositories;
using yazlab1proje3webapi.Repositories.CustomerRepositories;
using yazlab1proje3webapi.Repositories.LogRepositories;
using yazlab1proje3webapi.Repositories.OrderRepositories;
using yazlab1proje3webapi.Repositories.ProductRepositories;
using yazlab1proje3webapi.Repositories.SepetimRepositories;
using yazlab1proje3webapi.Repositories.VeriRepositories;
using yazlab1proje3webapi.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTransient<Context>();
builder.Services.AddTransient<Hash>();
builder.Services.AddTransient<SemaphoreClass>();
builder.Services.AddTransient<IProductRepository,ProductRepository>();
builder.Services.AddTransient<IOrderRepository,OrderRepository>();
builder.Services.AddTransient<ICustomerRepository,CustomerRepository>();    
builder.Services.AddTransient<IAdminRepository,AdminRepository>();
builder.Services.AddTransient<ILogRepository, LogRepository>();
builder.Services.AddTransient<IVeriRepository, VeriRepository>();
builder.Services.AddTransient<ISepetimRepository, SepetimRepository>();
builder.Services.AddTransient<IOrderServices,OrderServices>();
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.WithOrigins("https://localhost:7020") 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
        //builder.AllowAnyHeader()
        //       .AllowAnyMethod()
        //       .SetIsOriginAllowed((host) => true)
        //       .AllowCredentials();

    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("CorsPolicy");

app.UseHttpsRedirection();
app.UseAuthentication();    
app.UseAuthorization();

app.MapControllers();

app.Run();
