using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Presentation.Extensions;
using Presentacion.Middleware;
using Core.Security.Domain.Entities;
using MinimalApi.Endpoint.Extensions;
using Core.Security.Application.Service.Authentication;
using Core.Security.Application.Mappings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpoints();

builder.Services
    .AddSqlServer<ErplaDBContext>(builder.Configuration.GetConnectionString("Default"))
    .AddIdentityCore<User>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ErplaDBContext>();

builder.Services.AddRepositories()
                .AddBusinessServices();

builder.Services
    .AddHttpContextAccessor()
    .AddAuthorization()
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("claims.create",
        policy => policy.RequireClaim("Permission", "claims.create"));
    options.AddPolicy("roles.view",
    policy => policy.RequireClaim("Permission", "roles.view"));
});

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(MappingsProfile));
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api", Version = "v1" });
    c.EnableAnnotations();
});

var app = builder.Build();

// configure HTTP request pipeline
{
    // global cors policy
    // ToDo: set CORS to Dev only.
    app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
}

app.UseMiddleware<ErrorHandlerMiddleware>();
app.MapControllers();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

var serviceProvider = builder.Services.BuildServiceProvider();
{
    var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    IdentityDataSeeder.SeedData(userManager, roleManager);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1"));
}

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.MapEndpoints();
app.Run();