using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Text;

namespace Demo_RateLimiting.Middleware.Extensions;

public static class CachingExtensions
{
    public async static Task SetCahceValueAsync<T>(this IDistributedCache distributedCache, string key, T value, CancellationToken token = default)
    {
        await distributedCache.SetAsync(key, value.ToByteArray(), token);
    }

    public async static Task<T> GetCacheValueAsync<T>(this IDistributedCache distributedCache, string key, CancellationToken token = default) where T : class
    {
        var result = await distributedCache.GetAsync(key, token);
        return result.FromByteArray<T>();
    }

    public static byte[] ToByteArray(this object objectToSerialize)
    {
        if (objectToSerialize == null)
        {
            return null;
        }

        return Encoding.Default.GetBytes(JsonConvert.SerializeObject(objectToSerialize));
    }

    public static T FromByteArray<T>(this byte[] arrayToDeserialize) where T : class
    {
        if (arrayToDeserialize == null)
        {
            return default;
        }

        return JsonConvert.DeserializeObject<T>(Encoding.Default.GetString(arrayToDeserialize));
    }


}

