//
// Microsoft.Web.Services.Addressing.Address.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Net;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{

	public class Address : AttributedUri, IXmlElement
	{
		
		public Address (Uri uri) : base (uri)
		{	
		}
		
		public Address (XmlElement element) : base ()
		{
			LoadXml (element);
		}
		
		public XmlElement GetXml (XmlDocument document)
		{
			if(document == null) {
				throw new ArgumentNullException ("document");
			}

			XmlElement element = document.CreateElement ("wsa",
			                                             "Address",
								     "http://schemas.xmlsoap.org/ws/2003/03/addressing");
			
			GetXmlUri (document, element);

			return element;
			
		}

		public void LoadXml (XmlElement element)
		{

			if(element == null) {
				throw new ArgumentNullException ("element");
			}
			if(element.LocalName != "Address" || element.NamespaceURI != "http://schemas.xmlsoap.org/ws/2003/03/addressing") {
				throw new ArgumentException ("Invalid Element Supplied");
			}

			LoadXmlUri (element);
		
		}

		public static implicit operator Address (Uri uri)
		{
			return new Address(uri);
		}

		public static implicit operator Uri (Address address)
		{
			if(address == null) {
				return null;
			}
			return address.Value;
		}
	}
}
