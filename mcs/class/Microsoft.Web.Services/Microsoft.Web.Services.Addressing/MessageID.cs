//
// Microsoft.Web.Services.Addressing.MessageID.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{
	public class MessageID : AttributedUri, IXmlElement
	{
		public MessageID (Uri uri) : base (uri)
		{
		}

		public MessageID (XmlElement element) : base ()
		{
			LoadXml (element);
		}

		public MessageID () : base ( new Uri ("uuid:" + Guid.NewGuid ()))
		{
		}

		public XmlElement GetXml (XmlDocument document)
		{
			if(document == null) {
				throw new ArgumentNullException ("document");
			}

			XmlElement element = document.CreateElement ("wsa",
			                                             "MessageID",
								     "http://scemas.xmlsoap.org/2003/03/addressing");

			GetXmlUri (document, element);
			return element;
		}
		
		public void LoadXml (XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}
			if(element.LocalName != "MessageID" || element.NamespaceURI != "http://schemas.xmlsoap.org/2003/03/addressing") {
				throw new ArgumentException ("Invalid Element Supplied");
			}

			LoadXmlUri (element);
			
		}
	}
}
