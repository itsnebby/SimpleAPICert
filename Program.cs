using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using UserManagementAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var bearerToken = builder.Configuration["Authentication:BearerToken"];
if (!builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(bearerToken))
{
    throw new InvalidOperationException(
        "Configure Authentication:BearerToken through a secure configuration provider before starting outside Development.");
}

builder.Services.AddControllers();
builder.Services.AddSingleton<UserManagementAPI.Services.UserStore>();
builder.Services.AddProblemDetails();
builder.Services.AddAuthentication("Bearer")
    .AddScheme<AuthenticationSchemeOptions, BearerTokenAuthenticationHandler>("Bearer", _ => { });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder("Bearer")
        .RequireAuthenticatedUser()
        .Build();
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapGet("/_debug/throw", ThrowForMiddlewareTest);
}

// Middleware order matters: outer error handling catches downstream failures;
// authentication establishes the principal; logging wraps authorization and endpoints.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();

static IResult ThrowForMiddlewareTest()
{
    throw new InvalidOperationException("Development-only exception-handler test.");
}
