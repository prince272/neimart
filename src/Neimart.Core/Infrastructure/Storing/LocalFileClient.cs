using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neimart.Core;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Infrastructure.Storing
{
    public class LocalFileClient : IFileClient
    {
        private readonly string tempName = "temp";
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly string directoryPath;
        private readonly string directoryUrl;

        private readonly string[] allowedFileExtensions;
        private readonly long allowedFileSize;

        public LocalFileClient(IOptions<LocalFileClientOptions> fileClientOptionsAccessor, IActionContextAccessor httpContextAccessor)
        {

            _actionContextAccessor = httpContextAccessor;

            directoryPath = fileClientOptionsAccessor.Value.BasePath;
            directoryUrl = fileClientOptionsAccessor.Value.BaseUrl;

            allowedFileExtensions = fileClientOptionsAccessor.Value.AllowedFileExtensions;
            allowedFileSize = fileClientOptionsAccessor.Value.AllowedFileSize;
        }

        public async Task CreateAsync(string directoryName, string fileName, Stream source)
        {
            CheckFileExtension(fileName);
            CheckFileSize(source);

            string sourceFilePath = GetSourceFilePath(directoryName, fileName);

            var file = File.Open(sourceFilePath, FileMode.CreateNew);
            await source.CopyToAsync(file);
            await file.DisposeAsync();
        }

        public async Task ReplaceAsync(string directoryName, string fileName, Stream source)
        {
            await DeleteAsync(directoryName, fileName);
            await CreateAsync(directoryName, fileName, source);
        }


        public Task PrepareAsync(string directoryName, string fileName)
        {
            CheckFileExtension(fileName);

            string tempFilePath = GetTempFilePath(directoryName, fileName);
            var file = File.Open(tempFilePath, FileMode.CreateNew);
            return file.DisposeAsync().AsTask();
        }

        public async Task<bool> PatchAsync(string directoryName, string fileName, Stream source, long offset, long length)
        {
            CheckFileSize(source);

            string tempFilePath = GetTempFilePath(directoryName, fileName);
            string sourceFilePath = GetSourceFilePath(directoryName, fileName);

            var destination = new FileStream(tempFilePath, FileMode.Open, FileAccess.Write);
            destination.Seek(offset, SeekOrigin.Begin);

            CheckFileSize(destination);

            await source.CopyToAsync(destination);
            bool complete = destination.Length == length;
            await destination.DisposeAsync();

            if (complete)
            {
                File.Move(tempFilePath, sourceFilePath);
            }

            return complete;
        }

        public Task<bool> DeleteAsync(string directoryName, string fileName)
        {
            string tempFilePath = GetTempFilePath(directoryName, fileName);
            string sourceFilePath = GetSourceFilePath(directoryName, fileName);

            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);

            if (File.Exists(sourceFilePath))
            {
                File.Delete(sourceFilePath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<Stream> GetAsync(string directoryName, string fileName)
        {
            string sourceFilePath = GetSourceFilePath(directoryName, fileName);

            if (File.Exists(sourceFilePath))
            {
                var source = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.Read);
                return Task.FromResult<Stream>(source);
            }

            return Task.FromResult<Stream>(null);
        }

        private void CheckFileExtension(string fileName)
        {
            string fileExtension = Path.GetExtension(fileName);

            if (!allowedFileExtensions.Any(x => x.Equals(fileExtension, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new InvalidOperationException($"This file extension '{fileName}' is not allowed.");
            }
        }

        private void CheckFileSize(Stream source)
        {
            if (source.Length > allowedFileSize)
            {
                throw new InvalidOperationException("The file size has exceeded the limit allowed.");
            }
        }

        public string GetSourceUrl(string directoryName, string fileName)
        {
            if (directoryName == null || fileName == null)
                return null;

            var factory = _actionContextAccessor.ActionContext.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
            var urlHelper = factory.GetUrlHelper(_actionContextAccessor.ActionContext);
            string sourceUrl = urlHelper.ContentLink(WebPathHelper.CombineUrlParts(directoryUrl, directoryName, fileName));
            return sourceUrl;
        }

        private string GetSourceFilePath(string directoryName, string fileName)
        {
            if (directoryName == null)
                throw new ArgumentNullException(nameof(directoryName));

            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            string sourcePath = Path.Combine(directoryPath, directoryName);

            if (!Directory.Exists(sourcePath))
                Directory.CreateDirectory(sourcePath);

            return Path.Combine(sourcePath, fileName);
        }

        private string GetTempFilePath(string directoryName, string fileName)
        {
            if (directoryName == null)
                throw new ArgumentNullException(nameof(directoryName));

            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            string sourcePath = Path.Combine(directoryPath, directoryName, tempName);

            if (!Directory.Exists(sourcePath))
                Directory.CreateDirectory(sourcePath);

            return Path.Combine(sourcePath, fileName);
        }

        #region IAsyncDisposable Support
        public virtual ValueTask DisposeAsync()
        {
            Dispose();
            return default;
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

        ~LocalFileClient()
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