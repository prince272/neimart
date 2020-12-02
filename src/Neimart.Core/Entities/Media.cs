using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Neimart.Core.Entities
{
    public class Media : IEntity
    {
        public int Position { get; set; }

        public long Id { get; set; }

        public string FileTitle { get; set; }

        public string FileName { get; set; }

        public long FileSize { get; set; }

        public string FileType { get; set; }

        public string FileExtension { get; set; }

        public string ContentType { get; set; }

        public MediaType MediaType { get; set; }

        public string DirectoryName { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public Media AsMedia()
        {
            var media = new Media
            {
                Position = Position,
                Id = Id,
                FileTitle = FileTitle,
                FileName = FileName,
                FileSize = FileSize,
                FileType = FileType,
                ContentType = ContentType,
                DirectoryName = DirectoryName,
                Width = Width,
                Height = Height
            };
            return media;
        }
    }

    public enum MediaType
    {
        Image,
        Audio,
        Video,
        Document
    }
}
