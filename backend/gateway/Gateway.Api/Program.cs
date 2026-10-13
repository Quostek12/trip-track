var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(config =>
    {
        config.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway Service"); // GATEWAY
        
        config.SwaggerEndpoint("http://localhost:7001/swagger/v1/swagger.json", "Resources Service"); // RESOURCES
        config.SwaggerEndpoint("http://localhost:7002/swagger/v1/swagger.json", "Workers Service"); // WORKERS
        config.SwaggerEndpoint("http://localhost:7003/swagger/v1/swagger.json", "Trips Service"); // TRIPS
    });
}

app.UseHttpsRedirection();
app.MapReverseProxy();
app.UseCors("AllowFrontend");
app.Run();
