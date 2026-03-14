using RaportProdukcji.Components;
using RaportProdukcji.Services;
using RaportProdukcji.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Default"))
    )
);

// Add production service (uses database connection from configuration)
builder.Services.AddScoped<IProductionService, ProductionService>();
builder.Services.AddScoped<IPalletService, PalletService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Add Logger Service (Singleton to persist logs in memory)
builder.Services.AddSingleton<ILoggerService, LoggerService>();

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

// Add Logger API endpoints
var loggerService = app.Services.GetRequiredService<ILoggerService>();

// POST /_api/logs - receive logs from client
app.MapPost("/_api/logs", async (LogEntry logEntry, ILoggerService logger) =>
{
    logger.LogMessage(logEntry.Type, logEntry.Message, logEntry.Details);
    return Results.Ok();
});

// GET /_api/logs - get all logs
app.MapGet("/_api/logs", (ILoggerService logger, string? type) =>
{
    var logs = logger.GetLogs(type);
    return Results.Ok(logs);
});

// GET /_api/logs/stats - get log statistics
app.MapGet("/_api/logs/stats", (ILoggerService logger) =>
{
    var stats = logger.GetStats();
    return Results.Ok(stats);
});

// DELETE /_api/logs - clear logs
app.MapDelete("/_api/logs", (ILoggerService logger) =>
{
    logger.ClearLogs();
    return Results.Ok();
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
