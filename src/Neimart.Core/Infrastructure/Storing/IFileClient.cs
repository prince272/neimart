using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Neimart.Core.Infrastructure.Storing
{
    public interface IFileClient : IDisposable, IAsyncDisposable
    {
        Task CreateAsync(string directoryName, string fileName, Stream source);

        Task ReplaceAsync(string directoryName, string fileName, Stream source);

        Task PrepareAsync(string directoryName, string fileName);

        Task<bool> PatchAsync(string directoryName, string fileName, Stream source, long offset, long length);

        Task<bool> DeleteAsync(string directoryName, string fileName);

        Task<Stream> GetAsync(string directoryName, string fileName);

        string GetSourceUrl(string directoryName, string fileName);
    }
}
