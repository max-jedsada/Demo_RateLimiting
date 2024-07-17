using AspNetCoreRateLimit;
using Demo_RateLimiting.Middleware;
using Demo_RateLimiting.Middleware.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

var CurrentEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RateLimiting.API, : " + CurrentEnvironment,
        Version = "v1"
    });
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(600);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();

var app = builder.Build();
var _env = app.Environment.EnvironmentName;

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(q => q.SwaggerEndpoint("v1/swagger.json", _env));
}

app.UseCors(x => x
     .SetIsOriginAllowed(origin => true)
     .AllowAnyMethod()
     .AllowAnyHeader()
     .AllowCredentials());

app.UseRouting();

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseSession();

app.UseMiddleware<RateLimitingMiddleware>(); // use middleware

app.Run();
