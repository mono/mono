//
// Microsoft.Web.Services.Addressing.AttributedQName.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{

	public class AttributedQName : OpenAttributeElement
	{

		private QualifiedName _qname;

		public AttributedQName (QualifiedName qname) : base ()
		{
			if(qname == null) {
				throw new ArgumentNullException ("qname");
			}
			_qname = qname;
		}

		public AttributedQName () : base ()
		{
		}

		public void GetXmlQName (XmlDocument document, XmlElement element)
		{
			if(document == null) {
				throw new ArgumentNullException ("document");
			}
			if(element == null) {
				throw new ArgumentNullException ("document");
			}

			GetXmlAny (document, element);
			_qname.GetQualifiedName (document, element);
		}

		public void LoadXmlQName (XmlElement element)
		{
			ValidateSchema (element);

			LoadXmlAny (element);

			_qname = QualifiedName.FromString (element.InnerText, element);
		}

		public void ValidateSchema (XmlElement element)
		{
			if(element.ChildNodes.Count > 1) {
				throw new AddressingFormatException ("wsa_InvalidQName");
			}
		}

		public QualifiedName Value {
			get { return _qname; }
			set { _qname = value; }
		}

	}

}
