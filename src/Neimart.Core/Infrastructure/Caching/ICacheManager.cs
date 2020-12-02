using System;
using System.Threading.Tasks;

namespace Neimart.Core.Infrastructure.Caching
{
    public interface ICacheManager : IDisposable, IAsyncDisposable
    {
        Task ClearAsync();
        T Get<T>(string key, Func<T> acquire, int? cacheTime = null);
        Task<T> GetAsync<T>(string key, Func<Task<T>> acquire, int? cacheTime = null);
        Task<bool> IsSetAsync(string key);
        Task<bool> PerformActionWithLockAsync(string key, TimeSpan expirationTime, Action action);
        Task RemoveAsync(string key);
        Task RemoveByPrefixAsync(string prefix);
        Task SetAsync(string key, object data, int cacheTime);
        ValueTask<string> ComposeKeyAsync(string prefix, params object[] values);
    }
}