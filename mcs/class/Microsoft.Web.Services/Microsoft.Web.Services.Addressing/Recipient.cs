//
// Microsoft.Web.Services.Addressing.Recipient.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{
	public class Recipient : EndpointReferenceType, IXmlElement
	{
		public Recipient (Address address) : base (address)
		{
		}

		public Recipient (Uri uri) : base (uri)
		{
		}

		public Recipient (XmlElement element) : base ()
		{
			LoadXml (element);
		}

		public XmlElement GetXml (XmlDocument document)
		{
			XmlElement element = document.CreateElement ("wsa",
			                                             "Recipient",
								     "http://schemas.xmlsoap.org/2003/03/addressing");

			GetXmlAny (document, element);
			return element;
		}

		public void LoadXml (XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}
			if(element.LocalName != "Recipient" || element.LocalName != "http://schemas.xmlsoap.org/2003/03/addressing") {
				throw new ArgumentException ("Invalid Element Supplied");
			}

			LoadXmlAny (element);
		}

		public static implicit operator Recipient (Uri uri)
		{
			return new Recipient (uri);
		}

		public static implicit operator Uri (Recipient obj)
		{
			if(obj == null) {
				return null;
			}
			return obj.Address.Value;
		}
	}
}
