//
// Microsoft.Web.Services.Addressing.ReplyTo
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{

	public class ReplyTo : EndpointReferenceType, IXmlElement
	{

		public ReplyTo (Address address) : base (address)
		{
		}
		
		public ReplyTo (Uri address) : base (address)
		{
		}

		public ReplyTo (XmlElement element) : base ()
		{
			LoadXml (element);
		}
		
		public XmlElement GetXml (XmlDocument document)
		{
			if(document == null) {
				throw new ArgumentNullException ("document");
			}

			XmlElement element = document.CreateElement ("wsa",
			                                             "ReplyTo",
								     "http://schemas.xmlsoap.org/2003/03/addressing");
			GetXmlAny (document, element);
			return element;
		}

		public void LoadXml (XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}

			if(element.LocalName != "ReplyTo" || element.NamespaceURI != "http://schemas.xmlsoap.org/2003/03/addressing") {
				throw new ArgumentException ("Invalid Argument Supplied");
			}

			LoadXmlAny (element);
		}

		public static implicit operator ReplyTo (Uri uri)
		{
			return new ReplyTo (uri);
		}

		public static implicit operator Uri (ReplyTo obj)
		{
			if(obj == null) {
				return null;
			}
			return obj.Address.Value;
		}

	}

}
