//
// Microsoft.Web.Services.IXmlElement.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian Inc, 2003.
//

using System.Xml;

namespace Microsoft.Web.Services  {

        public interface IXmlElement
        {
                XmlElement GetXml (XmlDocument document);

                void LoadXml (XmlElement element);
        }
}
