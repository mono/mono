//
// Microsoft.Web.Services.Addressing.ReferenceProperties.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{

	public class ReferenceProperties : OpenElementElement, IXmlElement
	{
		public ReferenceProperties (XmlElement element) : base ()
		{
			LoadXml (element);
		}

		public ReferenceProperties () : base ()
		{
		}

		public XmlElement GetXml (XmlDocument document)
		{
			if(document == null) {
				throw new ArgumentNullException ("document");
			}

			XmlElement element = document.CreateElement ("wsa",
			                                             "ReferenceProperties",
								     "http://schemas.xmlsoap.org/ws/2003/03/addressing");
			
			GetXmlAny (document, element);
			return element;
		}
		
		public void LoadXml (XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}

			if(element.LocalName != "ReferenceProperties" || element.NamespaceURI != "http://schemas.xmlsoap.org/ws/2003/03/addressing") {
				throw new ArgumentException ("Invalid Element Supplied");
			}

			LoadXmlAny (element);
		}
	}

}
