using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Sms;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Web Host Defaults
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// Add services to the container.
Action<DbContextOptionsBuilder> buildDbContextOptions = o =>
    o.UseLazyLoadingProxies()
    .UseSqlServer(builder.Configuration.GetConnectionString("PhoenixConnection"));

builder.Services.AddDbContext<ApplicationContext>(buildDbContextOptions);
builder.Services.AddDbContext<PhoenixContext>(buildDbContextOptions);

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(o =>
    {
        o.User.AllowedUserNameCharacters = null;
        o.Password.RequireDigit = false;
        o.Password.RequireLowercase = false;
        o.Password.RequireNonAlphanumeric = false;
        o.Password.RequireUppercase = false;
        o.Password.RequiredLength = 6;
    })
    .AddRoles<ApplicationRole>()
    .AddUserStore<ApplicationStore>()
    .AddUserManager<ApplicationUserManager>()
    .AddEntityFrameworkStores<ApplicationContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => 
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// TODO: Make sure this is not needed
// builder.Services.AddCors();

builder.Services.AddScoped<ISmsService>(_ => 
    new SmsService(builder.Configuration["NexmoSMS:ApiKey"], builder.Configuration["NexmoSMS:ApiSecret"]));
builder.Services.AddHttpsRedirection(options => options.HttpsPort = 443);

builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:ConnectionString"]);

builder.Services.AddControllers()
    .AddNewtonsoftJson();

// TODO: Write detailed documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Logging
// TODO: Create File Logging & on app insights
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Logging.AddSimpleConsole();
builder.Logging.AddDebug();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // app.UseDatabaseErrorPage();
}
else
{
    app.UseHsts();
}

// TODO: Hide Swagger documentation
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseRouting();

app.UseAuthentication();

// TODO: What is this?
//app.Use(async (context, next) =>
//{
//    if (context is null)
//        throw new ArgumentNullException(nameof(context));

//    var logger = context.RequestServices.GetService<ILogger>();
//    if (logger is null)
//        return;

//    var claimsPrincipal = context?.User;
//    if (claimsPrincipal is null)
//    {
//        logger.LogTrace("No authorized user is set");
//        return;
//    }

//    logger.LogTrace("{NameIdentifier}: {NameIdentifierValue}", nameof(ClaimTypes.NameIdentifier),
//        claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
//    logger.LogTrace("{Role}s: {RoleValue}", nameof(ClaimTypes.Role),
//        string.Join(", ", claimsPrincipal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)));

//    await next(context!);
//});

app.UseAuthorization();

app.MapControllers();

app.Run();
