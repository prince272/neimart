using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Infrastructure.Storing
{
    public class LocalFileClientOptions
    {
        public string BasePath { get; set; }

        public string BaseUrl { get; set; }

        public string[] AllowedFileExtensions { get; set; }

        public long AllowedFileSize { get; set; }
    }
}
