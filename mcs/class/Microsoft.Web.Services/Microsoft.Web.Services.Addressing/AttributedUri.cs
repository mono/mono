//
// Microsoft.Web.Services.Addressing.AttributedUri
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

	public abstract class AttributedUri : OpenAttributeElement
	{

		private Uri _value;

		public AttributedUri (Uri uri) : base ()
		{
			if(uri == null) {
				throw new ArgumentNullException ("uri");
			}

			_value = uri;
		}

		public AttributedUri (AttributedUri aUri) : base ()
		{

			_value = aUri.Value;

			foreach (XmlAttribute attribute in aUri.AnyAttributes) {

				AnyAttributes.Add (attribute);

			}

		}

		public AttributedUri () : base ()
		{
		}

		public void GetXmlUri (XmlDocument document, XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}

			element.InnerText = _value.ToString();

			GetXmlAny(document, element);
		}

		public void LoadXmlUri (XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}

			ValidateSchema (element);

			LoadXmlAny (element);

			_value = new Uri(element.InnerText);

		}

		public void ValidateSchema (XmlElement element)
		{
			if(element.ChildNodes.Count > 1) {
				throw new AddressingFormatException ("wsa_InvalidAttributeUri");
			}
			if(element.ChildNodes.Count == 1 && !(element.FirstChild is XmlText)) {
				throw new AddressingFormatException ("wsa_InvalidAttributeUri");
			}
		}

		public Uri Value {
			get { return _value; }
			set { _value = value; }
		} 

	}

}
