//
// Microsoft.Web.Services.Addressing.FaultTo.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{
	public class FaultTo : EndpointReferenceType, IXmlElement
	{
		public FaultTo (Address address) : base (address)
		{
		}

		public FaultTo (Uri uri) : base (uri)
		{
		}

		public FaultTo (XmlElement element) : base ()
		{
			LoadXml (element);
		}

		public XmlElement GetXml (XmlDocument document)
		{
			XmlElement element = document.CreateElement ("wsa",
			                                             "FaultTo",
			                                             "http://schemas.xmlsoap.org/2003/03/addressing");

			GetXmlAny (document, element);
			return element;
		}

		public void LoadXml (XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}
			if(element.LocalName != "FaultTo" || element.NamespaceURI != "http://schemas.xmlsoap.org/2003/03/addressing") {
				throw new ArgumentException ("Invalid Element Supplied");
			}
			LoadXmlAny (element);
		}

		public static implicit operator FaultTo (Uri uri)
		{
			return new FaultTo (uri);
		}

		public static implicit operator Uri (FaultTo obj)
		{
			if(obj == null) {
				return null;
			}
			return obj.Address.Value;
		}
	}
}
