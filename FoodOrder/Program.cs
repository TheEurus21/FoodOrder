using FoodOrder.Application.Interfaces;
using FoodOrder.Infrastructure.Data;
using FoodOrder.Infrastructure.Repositories;
using FoodOrder.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using MassTransit;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using FoodOrder.Infrastructure.Middleware;
using FoodOrder.Application.Services;
using FoodOrder.Application.Options;
using FoodOrder.Infrastructure.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((ctx, lc) => lc
.Enrich.FromLogContext()
.WriteTo.Console() .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200")) 
{
    AutoRegisterTemplate = true,
    IndexFormat = $"foodorder-logs-{DateTime.UtcNow:yyyy-MM}" 
}));

// Add services to the container.

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<OrderSmsSaga, OrderSmsSagaState>()
     .EntityFrameworkRepository(r =>
     {
         r.ConcurrencyMode = ConcurrencyMode.Pessimistic;

         r.AddDbContext<DbContext, SmsSagaDbContext>((provider, options) =>
         {
             options.UseSqlServer(
                 builder.Configuration.GetConnectionString("SagaConnection"),
                 sqlOptions => sqlOptions.MigrationsAssembly(typeof(SmsSagaDbContext).Assembly.FullName)
             );
         });
     });

    x.UsingActiveMq((context, cfg) =>
    {
        cfg.Host("localhost", 61616, h =>
        {
            h.Username("admin");
            h.Password("admin");
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid JWT token."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<SmsSagaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SagaConnection")));

builder.Services.Configure<BussinessRulesOptions>(
    builder.Configuration.GetSection("BussinessRules")
);

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
builder.Services.AddScoped<IFoodRepository, FoodRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPasswordHasherService, IdentityPasswordHasherService>();
builder.Services.AddScoped<IPasswordHasherService, BCryptPasswordHasherService>();
builder.Services.AddScoped<PasswordHasherFactory>();
builder.Services.AddScoped<RestaurantService>();
builder.Services.AddHostedService<OrderCancelWorker>();


builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "FoodOrder_";
});

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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();
