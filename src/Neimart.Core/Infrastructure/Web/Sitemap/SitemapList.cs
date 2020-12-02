using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Neimart.Core.Infrastructure.Web.Sitemap
{
    [Serializable]
    [XmlRoot(ElementName = "sitemapindex", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
    public class SitemapList
    {
        public SitemapList()
        {
            Sitemaps = new List<Sitemap>();
        }

        /// <summary>
        /// Creates a sitemap index which serializes to a sitemapindex element of a sitemap index file: https://www.sitemaps.org/protocol.html#index 
        /// </summary>
        /// <param name="sitemaps">A list of sitemap metadata to include in the sitemap index.</param>
        public SitemapList(List<Sitemap> sitemaps)
        {
            Sitemaps = sitemaps;
        }

        [XmlElement("sitemap")]
        public List<Sitemap> Sitemaps { get; private set; }

        public virtual string ToXml()
        {
            var xmlSerializer = new XmlSerializer(typeof(SitemapList));

            using (var textWriter = new Utf8StringWriter())
            {
                xmlSerializer.Serialize(textWriter, this);
                return textWriter.ToString();
            }
        }

        public static SitemapList FromXml(string xml)
        {
            using (TextReader textReader = new StringReader(xml))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SitemapList));
                var sitemap = serializer.Deserialize(textReader);
                return sitemap as SitemapList;
            }
        }
    }
}
