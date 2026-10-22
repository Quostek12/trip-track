using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Security.Claims;
using Microsoft.OpenApi;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Secret"]!)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AuthenticatedUserPolicy", policy => policy.RequireAuthenticatedUser());
});

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy")).AddTransforms(builderContext =>
{
    builderContext.AddRequestTransform(async requestContext =>
    {
        var user = requestContext.HttpContext.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                requestContext.ProxyRequest.Headers.Remove("X-User-Id");
                requestContext.ProxyRequest.Headers.Add("X-User-Id", userId);
            }
            
            var roles = string.Join(".", user.FindAll(ClaimTypes.Role).Select(x => x.Value));
            if (!string.IsNullOrEmpty(roles))
            {
                requestContext.ProxyRequest.Headers.Remove("X-User-Roles");
                requestContext.ProxyRequest.Headers.Add("X-User-Roles", roles);
            }
        }
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT access token}"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(config =>
    {
        config.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway Service"); // GATEWAY
        
        config.SwaggerEndpoint("http://localhost:7000/swagger/v1/swagger.json", "Auth Service"); // AUTH
        config.SwaggerEndpoint("http://localhost:7001/swagger/v1/swagger.json", "Resources Service"); // RESOURCES
        config.SwaggerEndpoint("http://localhost:7002/swagger/v1/swagger.json", "Workers Service"); // WORKERS
        config.SwaggerEndpoint("http://localhost:7003/swagger/v1/swagger.json", "Trips Service"); // TRIPS
    });
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.MapReverseProxy();
app.UseCors("AllowFrontend");
app.Run();
