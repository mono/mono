//
// Microsoft.Web.Services.Addressing.RelatesTo.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{
	public class RelatesTo : AttributedUri, IXmlElement
	{
		private QualifiedName _type;

		public RelatesTo (XmlElement element) : base ()
		{
			_type = new QualifiedName ("wsa",
			                           "Response",
						   "http://schemas.xmlsoap.org/ws/2003/03/addressing");
			LoadXml (element);
		}

		public RelatesTo (Uri uri) : base (uri)
		{
			_type = new QualifiedName ("wsa",
			                           "Response",
						   "http://schemas.xmlsoap.org/ws/2003/03/addressing");
			if(uri == null) {
				throw new ArgumentNullException ("related");
			}
		}

		public XmlElement GetXml (XmlDocument document)
		{
			if(document == null) {
				throw new ArgumentNullException ("related");
			}
			XmlElement element = document.CreateElement ("wsa",
			                                             "RelatesTo",
								     "http://schemas.xmlsoap.org/ws/2003/03/addressing");
			if(_type != null) {
				XmlAttribute attrib = document.CreateAttribute ("RelationshipType");

				attrib.Value = _type.Value;

				element.Attributes.Append (attrib);

				if(_type.Namespace != "http://schemas.xmlsoap.org/ws/2003/03/addressing") {
					element.Attributes.Append (_type.GetNamespaceDecl (document));
				}
			}

			GetXmlUri (document, element);
			return element;
		}

		public void LoadXml (XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}

			if(element.LocalName != "RelatesTo" || element.NamespaceURI != "http://schemas.xmlsoap.org/ws/2003/03/addressing") {
				throw new ArgumentException ("Invalid Element Supplied");
			}

			ValidateSchema (element);

			_type = new QualifiedName ("wsa",
			                           "Response",
						   "http://schemas.xmlsoap.org/ws/2003/03/addressing");

			foreach(XmlAttribute attrib in element.Attributes) {
				if(attrib.LocalName == "RelationshipType") {
					_type = QualifiedName.FromString (attrib.InnerText, element);
				} else {
					AnyAttributes.Add (attrib);
				}
			}

			Value = new Uri (element.InnerText);
			
		}

		public static implicit operator RelatesTo (Uri uri)
		{
			return new RelatesTo (uri);
		}

		public static implicit operator Uri (RelatesTo obj)
		{
			if(obj == null) {
				return null;
			}
			return obj.Value;
		}
	}
}
