using Demo_RateLimiting.Middleware.Extensions;
using Demo_RateLimiting.Middleware.Model;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Net;

namespace Demo_RateLimiting.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;

        public RateLimitingMiddleware(RequestDelegate next, IDistributedCache cache, IConfiguration configuration)
        {
            _next = next;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var rateLimitingDecorator = endpoint?.Metadata.GetMetadata<LimitRequests>();

            var configurationRule = _configuration.GetSection("IpRateLimiting:GeneralRules").Get<List<ConfigurationModel>>();
            foreach (var item in configurationRule)
            {
                if (item.Method.ToLower() == context.Request.Method.ToLower() 
                    && item.Endpoint.ToLower() == context.Request.Path.ToString().ToLower())
                {
                    rateLimitingDecorator = new LimitRequests();
                    rateLimitingDecorator.Period = item.Period;
                    rateLimitingDecorator.Limit = item.Limit;
                }
            }

            if (rateLimitingDecorator is null)
            {
                await _next(context);
                return;
            }

            var key = GenerateClientKey(context);
            var clientStatistics = await GetClientStatisticsByKey(key);

            if (clientStatistics != null && DateTime.UtcNow < clientStatistics.LastSuccessfulResponseTime.AddSeconds(rateLimitingDecorator.Period) && clientStatistics.NumberOfRequestsCompletedSuccessfully == rateLimitingDecorator.Limit)
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                return;
            }

            await UpdateClientStatisticsStorage(key, rateLimitingDecorator.Limit);
            await _next(context);
        }

        private static string GenerateClientKey(HttpContext context) => $"{context.Request.Path}_{context.Connection.RemoteIpAddress}";

        private async Task<ClientStatistics> GetClientStatisticsByKey(string key) => await _cache.GetCacheValueAsync<ClientStatistics>(key);

        private async Task UpdateClientStatisticsStorage(string key, int maxRequests)
        {
            var clientStat = await _cache.GetCacheValueAsync<ClientStatistics>(key);

            if (clientStat != null)
            {
                clientStat.LastSuccessfulResponseTime = DateTime.UtcNow;

                if (clientStat.NumberOfRequestsCompletedSuccessfully == maxRequests)
                    clientStat.NumberOfRequestsCompletedSuccessfully = 1;

                else
                    clientStat.NumberOfRequestsCompletedSuccessfully++;

                await _cache.SetCahceValueAsync(key, clientStat);
            }
            else
            {
                var clientStatistics = new ClientStatistics
                {
                    LastSuccessfulResponseTime = DateTime.UtcNow,
                    NumberOfRequestsCompletedSuccessfully = 1
                };

                await _cache.SetCahceValueAsync(key, clientStatistics);
            }

        }
    }

    public class ClientStatistics
    {
        public DateTime LastSuccessfulResponseTime { get; set; }
        public int NumberOfRequestsCompletedSuccessfully { get; set; }
    }


}
