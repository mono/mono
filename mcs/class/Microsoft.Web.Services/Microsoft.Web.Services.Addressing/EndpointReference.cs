//
// Microsoft.Web.Services.Addressing.EndpointReference
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{

	public class EndpointReference : EndpointReferenceType, IXmlElement
	{

		public EndpointReference (Address address) : base (address)
		{
		}

		public EndpointReference (Uri uri) : base (uri)
		{
		}

		public EndpointReference (XmlElement element) : base ()
		{
			LoadXml (element);
		}

		public XmlElement GetXml (XmlDocument document)
		{
			if(document == null) {
				throw new ArgumentNullException ("document");
			}
			XmlElement element = document.CreateElement ("wsa",
			                                             "EndpointReference",
								     "http://schemas.xmlsoap.org/ws/2003/03/addressing");

			GetXmlAny (document, element);
			return element;
		}

		public void LoadXml (XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}
			if(element.LocalName != "EndpointReference" || element.NamespaceURI != "http://schemas.xmlsoap.org/ws/2003/03/addressing") {
				throw new ArgumentException ("Invalid Element Supplied");
			}
			LoadXmlAny (element);
		}

		public static implicit operator EndpointReference (Uri uri)
		{
			return new EndpointReference (uri);
		}

		public static implicit operator Uri (EndpointReference obj)
		{
			if(obj == null) {
				return null;
			}
			return obj.Address.Value;
		}

	}

}
