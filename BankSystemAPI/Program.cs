using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using Microsoft.EntityFrameworkCore;
using BankSystemAPI;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));

// Cors service
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
    builder =>
    {
        builder.AllowAnyOrigin();
        builder.AllowAnyMethod();
        builder.AllowAnyHeader();
    });
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "omran",
        ValidAudience = "all",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is my custom Secret key for authentication"))
    };
});

////this to record logs(old way) 
//Log.Logger = new LoggerConfiguration()
//            .WriteTo.Console()
//            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
//            .CreateLogger();
////Logging configs from Appsettings.json
Log.Logger = new LoggerConfiguration()
                 .ReadFrom.Configuration(builder.Configuration)
                 .CreateLogger();

// Add logging services
//builder.Services.AddLogging(loggingBuilder =>
//{
//    loggingBuilder.AddSerilog();
//});
builder.Host.UseSerilog();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

// Serilog request logging
app.UseSerilogRequestLogging();

// Cors middleware
app.UseCors("AllowAll");

app.UseAuthentication(); // JWT
app.UseAuthorization();

app.MapControllers();

app.Run();