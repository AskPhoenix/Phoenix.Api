using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Senders;
using System.Text;

// TODO: Unify Program class in all APIs

var builder = WebApplication.CreateBuilder(args);

// Configure Web Host Defaults
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// Add services to the container.
Action<DbContextOptionsBuilder> buildDbContextOptions = o => o
    .UseLazyLoadingProxies()
    .UseSqlServer(builder.Configuration.GetConnectionString("PhoenixConnection"));

builder.Services.AddDbContext<ApplicationContext>(buildDbContextOptions);
builder.Services.AddDbContext<PhoenixContext>(buildDbContextOptions);

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
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
            ValidateLifetime = true,
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// TODO: Make sure this is not needed
// builder.Services.AddCors();

builder.Services.AddScoped(_ =>
    new SmsSender(builder.Configuration["Vonage:Key"], builder.Configuration["Vonage:Secret"]));


builder.Services.AddApplicationInsightsTelemetry(
    o => o.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]);

builder.Services.AddControllers()
    .AddNewtonsoftJson();
builder.Services.AddHttpsRedirection(o => o.HttpsPort = 443);
builder.Services.AddRouting(o => o.LowercaseUrls = true);

// TODO: Write detailed documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenNewtonsoftSupport();
builder.Services.AddSwaggerGen(o =>
{
    o.EnableAnnotations();

    // SwaggerDoc name refers to the name of the documention and is included in the endpoint path
    o.SwaggerDoc("v3", new OpenApiInfo()
    {
        Title = "Egretta API",
        Description = "A Rest API to handle Phoenix backend data.",
        Version = "3.0"
    });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter the JWT Bearer token.",
        In = ParameterLocation.Header,
        Name = "JWT Authentication",
        Type = SecuritySchemeType.Http,

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    o.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

// Configure Logging
// TODO: Create File Logging & on app insights
builder.Logging.ClearProviders()
    .AddConfiguration(builder.Configuration.GetSection("Logging"))
    .SetMinimumLevel(LogLevel.Trace)
    .AddSimpleConsole()
    .AddDebug();


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
app.UseSwaggerUI(o => o.SwaggerEndpoint("/swagger/v3/swagger.json", "Egretta v3"));

app.UseHttpsRedirection();

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseRouting();

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
