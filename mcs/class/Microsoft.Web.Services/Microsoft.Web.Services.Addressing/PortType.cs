//
// Microsoft.Web.Services.Addressing.PortType.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{

	public class PortType : AttributedQName, IXmlElement
	{

		public PortType (XmlElement element) : base ()
		{
			LoadXml (element);
		}

		public PortType (QualifiedName qname) : base (qname)
		{
		}

		public XmlElement GetXml (XmlDocument document)
		{
			if(document == null) {
				throw new ArgumentNullException ("document");
			}
			XmlElement element = document.CreateElement("wsa",
			                                            "PortType",
								    "http://schemas.xmlsoap.org/ws/2003/03/addressing");
			GetXmlQName (document, element);
			
			return element;
		}

		public void LoadXml (XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}
			if(element.LocalName != "PortType" || element.NamespaceURI != "http://schemas.xmlsoap.org/ws/2003/03/addressing") {
				throw new ArgumentException ("Invalid Element Supplied");
			}
			LoadXmlQName (element);
		}

	}

}
