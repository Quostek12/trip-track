using Microsoft.EntityFrameworkCore;
using Shared.Services;
using Worker.Contracts;
using Workers.Api.Context;
using Workers.Api.Services;

var builder = WebApplication.CreateBuilder(args);

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
app.MapGrpcService<WorkerLookupService>(); 
app.UseCors("AllowGateway");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();