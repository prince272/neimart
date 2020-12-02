using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Neimart.Core.Infrastructure.Web.Sitemap
{
    [Serializable]
    [XmlRoot(ElementName = "urlset", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
    public class SitemapUrlList : List<SitemapUrl>
    {
        public virtual string ToXml()
        {
            var xmlSerializer = new XmlSerializer(typeof(SitemapUrlList));

            using (var textWriter = new Utf8StringWriter())
            {
                xmlSerializer.Serialize(textWriter, this);
                return textWriter.ToString();
            }
        }

        public static SitemapUrlList FromXml(string xml)
        {
            using (TextReader textReader = new StringReader(xml))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SitemapUrlList));
                var sitemap = serializer.Deserialize(textReader);
                return sitemap as SitemapUrlList;
            }
        }
    }


    /// <summary>
    /// Subclass the StringWriter class and override the default encoding.  
    /// This allows us to produce XML encoded as UTF-8. 
    /// </summary>
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}