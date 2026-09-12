
using Scalar.AspNetCore;
using SupportHub.Application.Contracts;
using SupportHub.Application.Services;

namespace SupportHub.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // RFC 9457 Problem Details: structured, machine-readable error bodies.
        builder.Services.AddProblemDetails();

        //TODO: Add the db context and other services here.
        builder.Services.AddScoped<ICustomerService, CustomerService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        //Not mandatory but a good practice to have a health check endpoint for liveness and readiness probes.
        app.MapGet("/health/live", () => Results.Ok(new { status = "live" }))
            .ExcludeFromDescription();

        app.Run();
    }
}
