//
// Microsoft.Web.Services.Xml.QualifiedName.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;

namespace Microsoft.Web.Services.Xml
{
	public class QualifiedName : XmlQualifiedName
	{

		private string _prefix;

		public QualifiedName (string prefix, string name, string namespaceURI) : base (name, namespaceURI)
		{
			if(prefix == null) {
				throw new ArgumentNullException ("prefix");
			}
			_prefix = prefix;
		}

		public static QualifiedName FromString (string value, XmlNode node)
		{
			if(node == null) {
				throw new ArgumentNullException ("node");
			}

			if(value.IndexOf(':') > 0) {
				string[] strings = value.Split(':');

				if(strings.Length != 2 || strings[0].Length == 0 || strings[1].Length == 0) {
					throw new FormatException ("xml_ImproperQName");
				}
				
				string nspace = node.GetNamespaceOfPrefix (strings[0]);

				if(nspace == null) {
					throw new FormatException ("xml_CouldNotResolveNSPrefix");
				}
				
				if(nspace.Length == 0) {
					throw new FormatException ("xml_CouldNotResolveNSPrefix");
				}

				return new QualifiedName (strings[0], strings[1], nspace);
			}
			
			throw new FormatException ("xml_ImproperQName");
		}

		public XmlAttribute GetNamespaceDecl (XmlDocument document)
		{
			if(document == null) {
				throw new ArgumentNullException ("document");
			}
			XmlAttribute attrib = document.CreateAttribute("xmlns",
			                                               _prefix,
								       "http://www.w3.org/2000/xmlns");

			attrib.Value = Namespace;
			return attrib;
		}

		public void GetQualifiedName (XmlDocument document, XmlElement element)
		{
			if(document == null) {
				throw new ArgumentNullException ("document");
			}
			if(element == null) {
				throw new ArgumentNullException ("element");
			}
			element.InnerText = Value;
			if(Namespace != element.NamespaceURI) {
				element.Attributes.Append(GetNamespaceDecl(document));
			}
		}

		public string Prefix {
			get { return _prefix; }
		}

		public string Value {
			get { return _prefix + ":" + Name; }
		}
	}
}
