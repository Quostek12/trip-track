using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Resources.Api.Context;
using System.Text.Json.Serialization;
using System.Reflection;
using Shared.Services;
using Resources.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(7001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
    options.ListenLocalhost(7101, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGateway", policy =>
    {
        policy.WithOrigins("http://localhost:5000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var connString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ResourcesDbContext>(options => options.UseNpgsql(connString));
builder.Services.AddGrpc();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers().AddJsonOptions(opt =>
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ResourcesDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors("AllowGateway");
app.MapGrpcService<ResourceLookupService>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();