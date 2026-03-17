var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FridayPosOptions>(builder.Configuration.GetSection(FridayPosOptions.SectionName));
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection(RedisOptions.SectionName));
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.SectionName));

builder.Services.AddSignalR();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "fridaypos-cloud66-poc",
    status = "ok",
    message = "FridayPOS-style ASP.NET Core service for Cloud 66 exploration"
}));

app.MapGet("/healthz", (IConfiguration config) => Results.Ok(new
{
    status = "healthy",
    environment = app.Environment.EnvironmentName,
    signalR = true,
    redisConfigured = !string.IsNullOrWhiteSpace(config["Redis:ConnectionString"]),
    rabbitMqConfigured = !string.IsNullOrWhiteSpace(config["RabbitMq:Host"]),
    storageConfigured = !string.IsNullOrWhiteSpace(config["Storage:BlobContainer"]),
    databaseConfigured = !string.IsNullOrWhiteSpace(config["ConnectionStrings:MainDb"])
}));

app.MapGet("/api/config", (IConfiguration config) => Results.Ok(new
{
    tenantName = config["FridayPos:TenantName"] ?? "FridayPOS Demo",
    region = config["FridayPos:Region"] ?? "unknown",
    enableRealtime = bool.TryParse(config["FridayPos:EnableRealtime"], out var enabled) && enabled
}));

app.MapGet("/api/ping", () => Results.Ok(new
{
    pong = true,
    source = "fridaypos-cloud66-poc",
    deployedAt = "2026-03-17"
}));

app.MapHub<NotificationsHub>("/hubs/notifications");

app.Run();

internal sealed class NotificationsHub : Microsoft.AspNetCore.SignalR.Hub
{
    public Task Ping() => Clients.Caller.SendCoreAsync("pong", new object?[] { "FridayPOS realtime channel is reachable" });
}

internal sealed class FridayPosOptions
{
    public const string SectionName = "FridayPos";

    public string TenantName { get; init; } = "FridayPOS Demo";

    public string Region { get; init; } = "uae-north";

    public bool EnableRealtime { get; init; } = true;
}

internal sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; init; } = string.Empty;
}

internal sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; init; } = string.Empty;
}

internal sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string BlobContainer { get; init; } = string.Empty;
}
