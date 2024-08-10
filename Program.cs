using BT_INTEGRATIONS.SQUARE;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Square;
using Square.Apis;
using Square.Exceptions;
using Square.Models;
using System.Reflection;

namespace BT_INTEGRATIONS.SQUARE;

public class Program
{

    public static string API = "";
    public static string SignatureKey = "";
    public static string EventURL = "";

    public async static Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        API = builder.Configuration["Api"];
        SignatureKey = builder.Configuration["SignatureKey"];
        EventURL = builder.Configuration["EventURL"];

        var StoreNumber = 0;
        var RegisterNumber = 0;

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        string? deviceCode = await SquareHandlers.GetDeviceCode();
        if (deviceCode == null)
        {
            return;
        }

        string? deviceStatus = await SquareHandlers.CheckPairStatus(deviceCode);
        if (deviceStatus == "UNKNOWN" || deviceStatus == "EXPIRED")
        {
            deviceCode = await SquareHandlers.GenerateNewDeviceCode();
        }


        app.Run();
    }
}
