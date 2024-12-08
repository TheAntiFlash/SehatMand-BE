using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySql.EntityFrameworkCore.Extensions;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Domain.Interface.Service;
using SehatMand.Domain.Utils.Agora;
using SehatMand.Domain.Utils.Smtp;
using SehatMand.Infrastructure.Extensions;
using SehatMand.Infrastructure.Persistence;
using SehatMand.Infrastructure.Repository;
using SehatMand.Infrastructure.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
var xmlDocsPath = Path.Combine(AppContext.BaseDirectory, typeof(Program).Assembly.GetName().Name + ".xml");

builder.Services.AddSwaggerGen(options =>
{
    //options.SwaggerDoc("v1", new OpenApiInfo { Title = "SehatMand.API", Version = "v1" });
    options.IncludeXmlComments(xmlDocsPath);
    options.SupportNonNullableReferenceTypes();
});

var config = builder.Configuration;
/*string? httpClientName = builder.Configuration["PmcHttpClient"];
ArgumentException.ThrowIfNullOrEmpty(httpClientName);*/

builder.Services.AddHttpClient(
    "PmcHttpClient", //httpClientName,
    client =>
    {
        // Set the base address of the named client.
        client.BaseAddress = new Uri("https://www.pmc.gov.pk/api/");
    });

builder.Services.AddHttpClient("OneSignal", client =>
{
    var apiKey = config.GetSection("OneSignal:ApiKey").Value ?? throw new Exception("OneSignal API key not found");
    
    client.BaseAddress = new Uri("https://api.onesignal.com"); 
    client.DefaultRequestHeaders.Add("Authorization", apiKey); 
});

builder.Services.AddHttpClient(
    
    "Agora",
    client =>
    {
        client.BaseAddress = new Uri("https://api.agora.io");
    });

builder.Services.Configure<SmtpSettings>(config.GetSection("SmtpSettings"));
builder.Services.Configure<AgoraSettings>(config.GetSection("AgoraSettings"));
builder.Services.AddDbContext<SmDbContext>(options => options.UseMySQL(config.GetConnectionString("SmDb")!));
builder.Services.AddScoped<DbContext, SmDbContext>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IDoctorVerificationService, DoctorVerificationService>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IMedicalForumRepository, MedicalForumRepository>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();
builder.Services.AddScoped<IFtpService, FtpService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMedicalHistoryRepository, MedicalHistoryRepository>();
builder.Services.AddScoped<IAgoraService, AgoraService>();
builder.Services.AddScoped<IStorageService, AwsService>();

//builder.Services.AddInfrastructure(config);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:7037";
        options.Audience = "SehatMand.Client";
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey
                (Encoding.UTF8.GetBytes(config.GetSection("JWT:Key").Value!)),
            ClockSkew = TimeSpan.Zero
        };
        
    });

builder.Services.AddAuthorization();
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) // allow any origin
    .AllowCredentials()); 

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();