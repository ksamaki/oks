using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapGet("/", () => new
{
    Service = "WaitMe.HelloService",
    Environment = app.Environment.EnvironmentName,
    Message = "Hello from WaitMe"
});

app.MapHealthChecks("/health");

app.Run();
