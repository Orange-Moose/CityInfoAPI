

// LEARN NEXT: https://app.pluralsight.com/library/courses/aspnet-core-fundamentals/table-of-contents


using CityInfo.API;
using CityInfo.API.Interfaces;
using CityInfo.API.Services;
using Microsoft.AspNetCore.StaticFiles;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/cityinfolog.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true; // Prevents returning response in default format (application.json) and gives 406 status instead
}).AddNewtonsoftJson() // configures input and output formatters and replaces default formatters form .NET
.AddXmlDataContractSerializerFormatters(); // Allows response in application.xml through Request Headers Accept: application.xml


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<FileExtensionContentTypeProvider>(); // register file extension content type provider for use in different parts of application

// compiler directives => omit of include peace of code on compile
#if DEBUG // if we run debug build LocalMailservice will be added to the container
builder.Services.AddTransient<IMailService, LocalMailService>(); // Register LocalMailService interface to be able to inject it
#else // if we run release build CloudMailservice will be added to the container
builder.Services.AddTransient<IMailService, CloudMailService>();
#endif

builder.Services.AddSingleton<CitiesDataStore>(); // register CitiesDataStore

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints => endpoints.MapControllers());

app.Run();
