using System;
using System.Xml.Serialization;

namespace Neimart.Core.Infrastructure.Web.Sitemap
{
    [Serializable]
    public class Sitemap
    {
        [NonSerialized]
        private readonly DateTime? _dateLastModified;

        private Sitemap()
        {
        }

        /// <summary>
        /// Creates a SitemapInfo object which serializes to the "sitemap" element of a sitemap index
        /// file: https://www.sitemaps.org/protocol.html#index 
        /// </summary>
        /// <param name="absolutePathToSitemap">
        /// The full path to the sitemap (e.g. https://www.somewebsite.com/sitemaps/sitemap1.xml). Serializes
        /// to the "loc" element.
        /// </param>
        /// <param name="dateSitemapLastModified">
        /// The date the sitemap was last modified/created. Serializes to the "lostmod" element.
        /// </param>
        public Sitemap(Uri absolutePathToSitemap, DateTime? dateSitemapLastModified = null)
        {
            AbsolutePathToSitemap = absolutePathToSitemap.ToString();
            _dateLastModified = dateSitemapLastModified;
        }

        /// <summary>
        /// The full path to the sitemap (e.g. https://www.somewebsite.com/sitemaps/sitemap1.xml).
        /// Serializes to the "loc" element.
        /// </summary>
        [XmlElement("loc")]
        public string AbsolutePathToSitemap { get; set; }

        /// <summary>
        /// The date the sitemap was last modified/created. Serializes to the "lostmod" element.
        /// </summary>
        [XmlElement("lastmod")]
        public string DateLastModified
        {
            get => _dateLastModified?.ToString("yyyy-MM-dd");
            set { }
        }

        public static Sitemap CreateSitemap(string absolutePathToSitemap, DateTime? dateSitemapLastModified = null)
        {
            return new Sitemap(new Uri(absolutePathToSitemap), dateSitemapLastModified);
        }
    }
}
