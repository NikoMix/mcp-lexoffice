using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Polly;
using Polly.RateLimit;
using Polly.Retry;
using System.ComponentModel;
using System.Net.Http;
using ModelContextProtocol;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Protocol;
using System.Threading;
using System.Net.Http.Headers;
using MixMedia.MCP.LexOffice.Services; // Assuming LexOfficeService is in this namespace
using System;

namespace MixMedia.MCP.LexOffice;
internal class Program
{
    static Task Main(string[] args) => 
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(serverOptions =>
                {
                    // Bind to localhost only for dev to prevent DNS rebinding
                    serverOptions.ListenLocalhost(5000);
                    serverOptions.AddServerHeader = false;
                });
                webBuilder.Configure(app =>
                {
                    // Middleware to validate Origin header for SSE endpoints
                    app.Use(async (context, next) =>
                    {
                        if (context.Request.Path.StartsWithSegments("/sse"))
                        {
                            var origin = context.Request.Headers["Origin"].ToString();
                            // Allow only trusted origins (adjust as needed)
                            if (!string.IsNullOrEmpty(origin) && !origin.StartsWith("http://localhost") && !origin.StartsWith("https://localhost"))
                            {
                                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                await context.Response.WriteAsync("Forbidden: Invalid Origin header.");
                                return;
                            }
                            // Simple authentication for SSE (e.g., via header)
                            var auth = context.Request.Headers["X-Api-Key"].ToString();
                            if (string.IsNullOrEmpty(auth) || auth != "YOUR_EXPECTED_API_KEY")
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                await context.Response.WriteAsync("Unauthorized: Missing or invalid API key.");
                                return;
                            }
                        }
                        await next();
                    });
                });
            })
            .ConfigureServices((context, services) =>
            {
                services.AddMcpServer()
                    .WithStdioServerTransport() // For local development, replace with .WithHttpTransport() for production
                    .WithHttpTransport(options =>
                    {
                        options.Stateless = true;
                    })
                    .WithToolsFromAssembly();

                var rateLimitPolicy = Policy.RateLimitAsync<HttpResponseMessage>(2, TimeSpan.FromSeconds(1));
                var retryPolicy = Policy<HttpResponseMessage>
                    .Handle<HttpRequestException>()
                    .OrResult(r => (int)r.StatusCode >= 500)
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

                services.AddHttpContextAccessor();
                services.AddTransient<ApiKeyToBearerHandler>();

                services.AddHttpClient("LexOfficeClient", client =>
                {
                    client.BaseAddress = new Uri("https://api.lexoffice.io/v1/");
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                })
                .AddHttpMessageHandler<ApiKeyToBearerHandler>()
                .AddPolicyHandler(rateLimitPolicy)
                .AddPolicyHandler(retryPolicy);

                services.AddLexOfficeService(options =>
                {
                    options.ApiKey = context.Configuration["LexOffice:ApiKey"]; // Assuming API key is in appsettings.json
                });

            }).Build().RunAsync();

    // DelegatingHandler to copy X-Api-Key to Authorization header
    public class ApiKeyToBearerHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ApiKeyToBearerHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Request.Headers.TryGetValue("X-Api-Key", out var apiKey) && !string.IsNullOrEmpty(apiKey))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}

public static class LexOfficeServiceExtensions
{
    public static IServiceCollection AddLexOfficeService(this IServiceCollection services, Action<LexOfficeServiceOptions> configureOptions)
    {
        var options = new LexOfficeServiceOptions();
        configureOptions(options);

        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new ArgumentNullException(nameof(options.ApiKey), "LexOffice API Key must be provided.");
        }

        // Register HttpClient for LexOfficeService using IHttpClientFactory
        // This is the recommended way to manage HttpClient instances.
        services.AddHttpClient<LexOfficeService>(client =>
        {
            // Base address and other default request headers (like Accept, Authorization) 
            // are configured within the LexOfficeService constructor itself.
            // client.BaseAddress = new Uri("https://api.lexoffice.io/v1/"); // This could be set here too.
        })
        // Optionally configure the primary message handler (e.g., for proxy, decompression)
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        });

        // Register LexOfficeService itself. It will be activated with the configured HttpClient.
        // The API key is passed to its constructor.
        // We use a factory function to correctly instantiate LexOfficeService with its dependencies.
        services.AddScoped(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            // Create a named HttpClient instance. The name should match what AddHttpClient used if it was named.
            // Or, if AddHttpClient<LexOfficeService> was used, this resolves the specifically configured client for LexOfficeService.
            var httpClient = httpClientFactory.CreateClient(typeof(LexOfficeService).Name); 
            
            return new LexOfficeService(httpClient, options.ApiKey);
        });

        return services;
    }
}

public class LexOfficeServiceOptions
{
    public string? ApiKey { get; set; }
}