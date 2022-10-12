

// LEARN NEXT: https://app.pluralsight.com/library/courses/asp-dot-net-core-6-securing-oauth-2-openid-connect/table-of-contents


using CityInfo.API;
using CityInfo.API.DbContexts;
using CityInfo.API.Interfaces;
using CityInfo.API.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;

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
builder.Services.AddSwaggerGen(setup =>
{
    var xmlCommentsFile = $"{Assembly.GetEntryAssembly().GetName().Name}.xml"; // Similar to, __dirname in nodejs. Returns "CityInfo.API.xml
    var xmlCommentFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile); // Combine cwd with filename

    setup.IncludeXmlComments(xmlCommentFullPath); // Don't forget to eneble "Documentation file path" option in Project => Properties

    //Add Authorization option to Swagger UI
    setup.AddSecurityDefinition("CityInfoApiBearerAuth", new OpenApiSecurityScheme()
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        Description = "Input a valid token to access the API"
    });

    // Send provided auth token as auth header in a request
    setup.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "CityInfoApiBearerAuth"
                    }
            }, new List<string>()
        }
    });
});


builder.Services.AddSingleton<FileExtensionContentTypeProvider>(); // register file extension content type provider for use in different parts of application

// compiler directives => omit of include peace of code on compile
#if DEBUG // if we run debug build LocalMailservice will be added to the container
builder.Services.AddTransient<IMailService, LocalMailService>(); // Register LocalMailService interface to be able to inject it
#else // if we run release build CloudMailservice will be added to the container
builder.Services.AddTransient<IMailService, CloudMailService>();
#endif

//builder.Services.AddSingleton<CitiesDataStore>(); // register CitiesDataStore (local DB imitation)

builder.Services.AddDbContext<CityInfoContext>(dbContextOptions => dbContextOptions.UseSqlite(
    builder.Configuration["ConnectionStrings:CityInfoDBConnectionString"])); // register CityInfo DB context and connect to DB appsettings file


//Register Repositories:
// AddScoped lifetime refference to dependency is created once per request. 
builder.Services.AddScoped<ICityInfoRepository, CityInfoRepository>();  //to register the repository we pass the contract (interface) and the implementation (repo). 

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());// Mount automapper package. /Profiles folder contents used here

// Register bearer token auth middleware to the container
builder.Services.AddAuthentication("Bearer") // add Bearer authentication strategy and its configuration
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidAudience = builder.Configuration["Authentication:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Authentication:SecretForKey"]))
        };
    });

// Register CBAC policy (Claim Based Access Control)
// The policy is added in the controller [Authorize] attribute, eg. [Authorize(Policy = "MustBeFromVilnius")]
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBeFromVilnius", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("city", "Vilnius");
    });
});

//Register and setup API versioning package
// The version is added in the controller [ApiVersion] attribute, eg. [ApiVersion("1.2")]
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0); // set default API version to "1.0"
    options.ReportApiVersions = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline by calling all the middlewares with "app.Use..."


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication(); // add bearer authentication middleware to the request pipeline

app.UseAuthorization();

app.UseEndpoints(endpoints => endpoints.MapControllers());

app.Run();
