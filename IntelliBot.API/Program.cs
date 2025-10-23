using IntelliBot.Application.Services;
using IntelliBot.Core.Interfaces;
using IntelliBot.Core.Models.Configuration;
using IntelliBot.Infrastructure.Clients;
using IntelliBot.Infrastructure.Repositories;
using IntelliBot.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/intellibot-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

// Register your services
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IOpenAIClient, OpenAIClient>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<ICacheService, CacheService>();

// Configuration
builder.Services.Configure<OpenAIConfig>(builder.Configuration.GetSection("OpenAI"));

// HttpClient for OpenAI
builder.Services.AddHttpClient<IOpenAIClient, OpenAIClient>((serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IOptions<OpenAIConfig>>().Value;
    client.BaseAddress = new Uri(config.BaseUrl);
    client.DefaultRequestHeaders.Add("User-Agent", "IntelliBot-API/1.0");
    client.Timeout = config.Timeout;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();