using A2A;
using A2A.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Add CORS support for WebAssembly client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebAssemblyClient", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable CORS
app.UseCors("AllowWebAssemblyClient");

app.UseHttpsRedirection();

// Create and setup the Echo Agent
var taskManager = new TaskManager();
var echoAgent = new EchoAgent.EchoAgent();
echoAgent.Attach(taskManager);

// Map the A2A endpoint
app.MapA2A(taskManager, "/echo");

// Add a simple health check endpoint
app.MapGet("/", () => new { 
    Service = "Echo Agent", 
    Version = "1.0.0", 
    A2AEndpoint = "/echo",
    Status = "Running" 
});

app.Run();
