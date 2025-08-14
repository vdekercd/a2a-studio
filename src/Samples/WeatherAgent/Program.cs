using System.ClientModel;
using A2A;
using A2A.AspNetCore;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using WeatherAgent.Services;

var builder = WebApplication.CreateBuilder(args);

var chatDeploymentName = builder.Configuration.GetValue<string>("AzureOpenAI:ChatDeploymentName");
var endpoint = builder.Configuration.GetValue<string>("AzureOpenAI:Endpoint");
var apiKey = builder.Configuration.GetValue<string>("AzureOpenAI:ApiKey");

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowA2AStudio", policy =>
    {
        policy.WithOrigins("https://a2astudio.net", "http://localhost:3000", "https://localhost:3000", "http://localhost:5076")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSingleton<IChatClient>
(provider => 
    new ChatClientBuilder(
            new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey)).GetChatClient(chatDeploymentName).AsIChatClient())
        .UseFunctionInvocation()
        .Build());

// Register services following dependency injection pattern
builder.Services.AddSingleton<IWeatherService, WeatherService>();
builder.Services.AddSingleton<ILocationDetectionService, LocationDetectionService>();

var app = builder.Build();

// Enable CORS
app.UseCors("AllowA2AStudio");

var agent = new WeatherAgent.WeatherAgent(
    app.Services.GetRequiredService<IChatClient>(),
    app.Services.GetRequiredService<IWeatherService>(),
    app.Services.GetRequiredService<ILocationDetectionService>());
var taskManager = new TaskManager();

agent.Attach(taskManager);

app.MapA2A(taskManager, "/agent");

Console.WriteLine("A2A Weather Agent is running on http://localhost:5000/agent");

await app.RunAsync();