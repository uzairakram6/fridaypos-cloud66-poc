using Npgsql;

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

app.MapGet("/healthz", async (IConfiguration config, CancellationToken cancellationToken) =>
{
    var dbCheck = await DatabaseHealthChecks.TryCheckDatabaseAsync(config, cancellationToken);

    return Results.Ok(new
    {
        status = dbCheck.reachable || !dbCheck.configured ? "healthy" : "degraded",
        environment = app.Environment.EnvironmentName,
        signalR = true,
        redisConfigured = !string.IsNullOrWhiteSpace(config["Redis:ConnectionString"]),
        rabbitMqConfigured = !string.IsNullOrWhiteSpace(config["RabbitMq:Host"]),
        storageConfigured = !string.IsNullOrWhiteSpace(config["Storage:BlobContainer"]),
        databaseConfigured = dbCheck.configured,
        databaseReachable = dbCheck.reachable,
        databaseError = dbCheck.error
    });
});

app.MapGet("/api/config", (IConfiguration config) => Results.Ok(new
{
    tenantName = config["FridayPos:TenantName"] ?? "FridayPOS Demo",
    region = config["FridayPos:Region"] ?? "unknown",
    enableRealtime = bool.TryParse(config["FridayPos:EnableRealtime"], out var enabled) && enabled,
    deploymentLabel = config["FridayPos:DeploymentLabel"] ?? "default"
}));

app.MapGet("/api/ping", () => Results.Ok(new
{
    pong = true,
    source = "fridaypos-cloud66-poc",
    deployedAt = "2026-03-17"
}));

app.MapGet("/healthz/db", async (IConfiguration config, CancellationToken cancellationToken) =>
{
    var dbCheck = await DatabaseHealthChecks.TryCheckDatabaseAsync(config, cancellationToken);

    return Results.Ok(new
    {
        configured = dbCheck.configured,
        reachable = dbCheck.reachable,
        error = dbCheck.error
    });
});

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

    public string DeploymentLabel { get; init; } = "default";
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

internal static class DatabaseHealthChecks
{
    public static async Task<(bool configured, bool reachable, string? error)> TryCheckDatabaseAsync(
        IConfiguration config,
        CancellationToken cancellationToken)
    {
        var connectionString = config.GetConnectionString("MainDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return (false, false, null);
        }

        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Timeout = 5,
                CommandTimeout = 5
            };

            await using var connection = new NpgsqlConnection(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand("select 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);

            return (true, true, null);
        }
        catch (Exception ex)
        {
            return (true, false, ex.Message);
        }
    }
}
