//
// Microsoft.Web.Services.Addressing.AttributedUriString
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{

	public abstract class AttributedUriString : OpenAttributeElement
	{
		
		private string _uri;

		public AttributedUriString () : base ()
		{
		}

		public AttributedUriString (string uri)
		{
			if(uri == null) {
				throw new ArgumentNullException ("uri");
			}
			_uri = uri;
		}

		public void GetXmlUri (XmlDocument document, XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}
			element.InnerText = _uri;
			GetXmlAny (document, element);
		}

		public void LoadXmlUri (XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}
			ValidateSchema (element);
			LoadXmlAny (element);
			_uri = element.InnerText;
		}

		public void ValidateSchema (XmlElement element)
		{
			if(element.ChildNodes.Count >= 2) {
				throw new AddressingFormatException ("wsa_InvalidAttributeUri");
			}
			if(element.ChildNodes.Count == 1 && !(element.FirstChild is XmlText)) {
				throw new AddressingFormatException ("wsa_InvalidAttributeUri");
			}
		}

		public string Value {
			get { return _uri; }
			set { _uri = value; }
		}
		
	}

}
