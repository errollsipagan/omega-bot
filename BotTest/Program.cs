using BotTest.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<BotControlService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/options", (BotControlService controlService) =>
{
    return Results.Ok(controlService.GetOptions());
});

app.MapGet("/api/state", (BotControlService controlService) =>
{
    return Results.Ok(controlService.GetState());
});

app.MapPost("/api/selection", (SelectionRequest request, BotControlService controlService) =>
{
    var result = controlService.UpdateSelection(request.Format, request.DeckId);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

app.MapPost("/api/duel/start", (BotControlService controlService) =>
{
    var result = controlService.StartDuel();
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

app.MapPost("/api/duel/rematch", (BotControlService controlService) =>
{
    var result = controlService.RequestRematch();
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

app.MapPost("/api/duel/reset", (BotControlService controlService) =>
{
    var result = controlService.ResetSelection();
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

app.MapFallbackToFile("index.html");

app.Run();
