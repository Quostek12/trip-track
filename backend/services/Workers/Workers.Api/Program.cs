using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Shared.Services;
using Worker.Contracts;
using Workers.Api.Context;
using Workers.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(7002, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
    options.ListenLocalhost(7102, listenOptions =>
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

builder.Services.AddDbContext<WorkersDbContext>(options => options.UseNpgsql(connString));


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddGrpc();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WorkersDbContext>();
    await db.Database.MigrateAsync();
}

app.MapGrpcService<WorkerLookupService>(); 
app.UseCors("AllowGateway");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();