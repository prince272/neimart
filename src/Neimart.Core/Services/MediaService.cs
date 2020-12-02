using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Infrastructure.Storing;
using Neimart.Core.Utilities.Extensions;
using Neimart.Core.Utilities.Helpers;
using Newtonsoft.Json;

namespace Neimart.Core.Services
{
    public class MediaService : EntityService<Media, MediaFilter>
    {
        private readonly IFileClient _fileClient;

        public MediaService(IServiceProvider services) : base(services)
        {
            _fileClient = services.GetRequiredService<IFileClient>();
        }

        protected override Task PrepareAsync(Media media)
        {
            var fileExtension = Path.GetExtension(media.FileName).ToLowerInvariant();
            var contentType = WebPathHelper.GetMimeType(fileExtension);
            var fileType = fileExtension.TrimStart('.').ToUpperInvariant();

            var comparisonType = StringComparison.InvariantCultureIgnoreCase;

            media.ContentType = contentType;
            media.FileType = fileType;
            media.FileExtension = fileExtension;
            media.MediaType =
                _appSettings.ImageFileExtensions.Any(x => string.Equals(x, fileExtension, comparisonType)) ? MediaType.Image :
                _appSettings.AudioFileExtensions.Any(x => string.Equals(x, fileExtension, comparisonType)) ? MediaType.Audio :
                _appSettings.VideoFileExtensions.Any(x => string.Equals(x, fileExtension, comparisonType)) ? MediaType.Video :
                _appSettings.DocumentFileExtensions.Any(x => string.Equals(x, fileExtension, comparisonType)) ? MediaType.Document :
                throw new InvalidOperationException();


            return Task.CompletedTask;
        }

        public ValueTask<string> GenerateExpiryTokenAsync(Media media)
        {
            var expiry = new MediaExpiry
            {
                DirectoryName = media.DirectoryName,
                FileName = media.FileName,
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(24)
            };
            var data = expiry.ToJsonString(camelCase: false);
            var encryptString = ComputeHelper.RijndaelOperation.Encrypt(data);
            return new ValueTask<string>(encryptString);
        }

        public ValueTask<MediaExpiry> GetExpiryFromTokenAsync(string token)
        {
            var data = ComputeHelper.RijndaelOperation.Decrypt(token);
            var expiry = data.ToJsonObject<MediaExpiry>(camelCase: false);
            return new ValueTask<MediaExpiry>(expiry);
        }

        public Task<Media> GetAsync(string directoryName, string fileName)
        {
            return GetAsync(new MediaFilter { DirectoryName = directoryName ?? string.Empty, FileName = fileName ?? string.Empty }); ;
        }

        public async Task<Media> GetAsync(MediaExpiry expiry)
        {
            if (!expiry.HasExpired())
            {
                var media = await GetAsync(expiry.DirectoryName, expiry.FileName);
                return media;
            }

            return null;
        }

        public Task PrepareSourceAsync(string directoryName, string fileName)
        {
            return _fileClient.PrepareAsync(directoryName, fileName);
        }

        public Task<bool> PatchSourceAsync(string directoryName, string fileName, Stream source, long offset, long length)
        {
            return _fileClient.PatchAsync(directoryName, fileName, source, offset, length);
        }

        public Task<Stream> GetSourceAsync(string directoryName, string fileName)
        {
            return _fileClient.GetAsync(directoryName, fileName);
        }

        public Task<Stream> GetSourceAsync(Media media)
        {
            return _fileClient.GetAsync(media.DirectoryName, media.FileName);
        }

        public Task ReplaceSourceAsync(string directoryName, string fileName, Stream source)
        {
            return _fileClient.ReplaceAsync(directoryName, fileName, source);
        }

        public Task DeleteSourceAsync(string directoryName, string fileName)
        {
            return _fileClient.DeleteAsync(directoryName, fileName);
        }

        public string GetSourceUrl(string directoryName, string fileName)
        {
            return _fileClient.GetSourceUrl(directoryName, fileName);
        }

        public override async Task ClearCacheAsync()
        {
            await _cacheManager.RemoveByPrefixAsync(EntityHelper.GetCachePrefix<Product>());
            await _cacheManager.RemoveByPrefixAsync(EntityHelper.GetCachePrefix<Banner>());
            await _cacheManager.RemoveByPrefixAsync(EntityHelper.GetCachePrefix<User>());

            await base.ClearCacheAsync();
        }

        public override IQueryable<Media> GetQuery(MediaFilter filter = null)
        {
            var query = _unitOfWork.Query<Media>();

            query = query.OrderByDescending(x => x);

            if (filter != null)
            {
                if (filter.FileName != null)
                {
                    query = query.Where(x => !string.IsNullOrWhiteSpace(x.FileName) && x.FileName == filter.FileName);
                }

                if (filter.DirectoryName != null)
                {
                    query = query.Where(x => !string.IsNullOrWhiteSpace(x.DirectoryName) && x.DirectoryName == filter.DirectoryName);
                }
            }

            return query;
        }
    }

    public class MediaExpiry
    {
        public MediaExpiry()
        {

        }

        public string DirectoryName { get; set; }

        public string FileName { get; set; }

        public DateTimeOffset ExpiresOn { get; set; }

        public bool HasExpired()
        {
            return DateTimeOffset.UtcNow > ExpiresOn;
        }
    }
}
