using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EasyCaching.Core;
using Neimart.Core;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Infrastructure.Caching
{
    public class CacheManager : ICacheManager
    {
        private readonly IEasyCachingProvider _provider;
        private readonly int CacheTime = 30;

        public CacheManager(IEasyCachingProvider provider)
        {
            _provider = provider;
        }

        public T Get<T>(string key, Func<T> acquire, int? cacheTime = null)
        {
            if (cacheTime <= 0)
                return acquire();

            return _provider.Get(key, acquire, TimeSpan.FromMinutes(cacheTime ?? CacheTime))
                .Value;
        }

        public async Task<T> GetAsync<T>(string key, Func<Task<T>> acquire, int? cacheTime = null)
        {
            if (cacheTime <= 0)
                return await acquire();

            var t = await _provider.GetAsync(key, acquire, TimeSpan.FromMinutes(cacheTime ?? CacheTime));
            return t.Value;
        }

        public async Task SetAsync(string key, object data, int cacheTime)
        {
            if (cacheTime <= 0)
                return;

            await _provider.SetAsync(key, data, TimeSpan.FromMinutes(cacheTime));
        }

        public Task<bool> IsSetAsync(string key)
        {
            return _provider.ExistsAsync(key);
        }

        public async Task<bool> PerformActionWithLockAsync(string key, TimeSpan expirationTime, Action action)
        {
            if (await _provider.ExistsAsync(key))
                return false;

            try
            {
                _provider.Set(key, key, expirationTime);

                action();

                return true;
            }
            finally
            {
                await RemoveAsync(key);
            }
        }

        public Task RemoveAsync(string key)
        {
            return _provider.RemoveAsync(key);
        }

        public Task RemoveByPrefixAsync(string prefix)
        {
            return _provider.RemoveByPrefixAsync(prefix);
        }

        public Task ClearAsync()
        {
            return _provider.FlushAsync();
        }

        // TODO: test compose key method.
        public ValueTask<string> ComposeKeyAsync(string prefix, params object[] values)
        {
            string composedKey = prefix;

            if (values != null && values.Any())
            {
                foreach (var value in values)
                {
                    if (value == null)
                    {
                        composedKey += "-null";
                    }
                    else if (value is IEnumerable)
                    {
                        var enumValues = ((IEnumerable)value).Cast<object>().Select(x => x?.ToString() ?? "null");
                        composedKey += $"-{{{string.Join(",", enumValues)}}}";
                    }
                    else if (value != null && !TypeHelper.IsSimpleType(value.GetType()))
                    {
                        var primValues = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(x => x.CanRead && x.CanWrite).Select(x => x.GetValue(value) ?? "null");
                        composedKey += $"-{{{string.Join(",", primValues)}}}";
                    }
                    else
                    {
                        composedKey += $"-{{{value}}}";
                    }
                }
            }

            return new ValueTask<string>(composedKey);
        }

        #region IAsyncDisposable Support
        public virtual ValueTask DisposeAsync()
        {
            try
            {
                Dispose();
                return default;
            }
            catch (Exception exception)
            {
                return new ValueTask(Task.FromException(exception));
            }
        }
        #endregion

        #region IDisposable Support
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            disposed = true;
        }

        ~CacheManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}