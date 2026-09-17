using VM.Server.API;
using VM.Server.API.Endpoints;
using VM.Server.API.Middleware;
using VM.Server.Repository;
using VM.Server.Repository.InMemory;

const string FrontendCorsPolicy = "frontend";

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddOpenApi();
builder.Services.Configure<VendingMachineOptions>(builder.Configuration.GetSection(VendingMachineOptions.SectionName));
builder.Services.AddVendingMachineBackend();

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapGet("/swagger", () => Results.Content(SwaggerUiPage.Html, "text/html"));
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors(FrontendCorsPolicy);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapExternalEndpoints();
app.MapProductEndpoints();
app.MapVendingEndpoints();

app.Run();

public partial class Program;
