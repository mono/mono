//
// Microsoft.Web.Services.Addressing.To
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{

	public class To : AttributedUri, IXmlElement
	{

		public To (AttributedUri uri) : base (uri)
		{
		}

		public To (Uri uri) : base (uri)
		{
		}

		public To (XmlElement element) : base ()
		{
			LoadXml (element);
		}
		
		public XmlElement GetXml (XmlDocument document)
		{
			if(document == null) {
				throw new ArgumentNullException ("document");
			}
			
			XmlElement element = document.CreateElement ("wsa",
			                                             "To",
								     "http://schemas.xmlsoap.org/2003/03/addressing");

			GetXmlUri (document, element);
			return element;
		}

		public void LoadXml (XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}

			if(element.LocalName != "To" || element.NamespaceURI != "http://schemas.xmlsoap.org/2003/03/addressing") {
				throw new ArgumentException ("Invalid Element Supplied");
			}

			LoadXmlUri (element);
		}

		public static implicit operator To (Uri uri)
		{
			return new To (uri);
		}

		public static implicit operator Uri (To obj)
		{
			if(obj == null) {
				return null;
			}
			return obj.Value;
		}

	}

}
