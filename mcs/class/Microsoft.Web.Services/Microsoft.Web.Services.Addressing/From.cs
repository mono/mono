//
// Microsoft.Web.Services.Addressing.From.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{
	public class From : EndpointReferenceType, IXmlElement
	{
		public From (Address address) : base (address)
		{
		}

		public From (Uri uri) : base (uri)
		{
		}

		public From (XmlElement element) : base ()
		{
			LoadXml (element);
		}

		public XmlElement GetXml (XmlDocument document)
		{
			XmlElement element = document.CreateElement ("wsa",
			                                             "From",
								     "http://schemas.xmlsoap.org/2003/03/addressing");

			GetXmlAny (document, element);
			return element;
		}

		public void LoadXml (XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}
			if(element.LocalName != "From" || element.LocalName != "http://schemas.xmlsoap.org/2003/03/addressing") {
				throw new ArgumentException ("Invalid Element Supplied");
			}

			LoadXmlAny (element);
		}

		public static implicit operator From (Uri uri)
		{
			return new From (uri);
		}

		public static implicit operator Uri (From obj)
		{
			if(obj == null) {
				return null;
			}
			return obj.Address.Value;
		}
	}
}
