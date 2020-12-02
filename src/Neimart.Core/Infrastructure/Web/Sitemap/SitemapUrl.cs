using System;
using System.Xml.Serialization;

namespace Neimart.Core.Infrastructure.Web.Sitemap
{
    [Serializable]
    [XmlRoot("url")]
    [XmlType("url")]
    public class SitemapUrl
    {
        [XmlElement("loc")]
        public string Location { get; set; }

        [XmlIgnore]
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Please do not use this property to change last modification date. 
        /// Use TimeStamp instead.
        /// </summary>
        [XmlElement("lastmod")]
        public string LastMod
        {
            get => TimeStamp.ToString("yyyy-MM-dd");
            set => TimeStamp = DateTime.Parse(value);
        }

        [XmlElement("changefreq")]
        public ChangeFrequency ChangeFrequency { get; set; }

        [XmlElement("priority")]
        public double Priority { get; set; }

        public SitemapUrl()
        {
        }

        public static SitemapUrl CreateUrl(string url, double priority = 0.5d, DateTime timeStamp = default)
        {
            return new SitemapUrl
            {
                Location = url,
                ChangeFrequency = ChangeFrequency.Daily,
                Priority = priority,
                TimeStamp = timeStamp == default ? DateTime.Now : timeStamp,
            };
        }
    }
}
