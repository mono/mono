//
// Microsoft.Web.Services.Xml.OpenElement.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using System.Collections;

namespace Microsoft.Web.Services.Xml
{

	public abstract class OpenElement
	{
		private ArrayList _anyAttribute;
		private ArrayList _anyElement;

		public OpenElement ()
		{
			_anyAttribute = new ArrayList ();
			_anyElement = new ArrayList ();
		}

		public void GetXmlAny (XmlDocument document, XmlElement element)
		{
			if(document == null) {
				throw new ArgumentNullException ("document");
			}
			if(element == null) {
				throw new ArgumentNullException ("element");
			}

			foreach(XmlAttribute attrib in AnyAttributes) {
				element.Attributes.Append ((XmlAttribute) document.ImportNode (attrib, true));
			}

			foreach(XmlElement elem in AnyElements) {
				element.AppendChild (document.ImportNode (elem, true));
			}
		}

		public void LoadXmlAny (XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}

			foreach(XmlAttribute attrib in element.Attributes) {
				AnyAttributes.Add (attrib);
			}

			foreach(XmlElement elem in element.ChildNodes) {
				AnyElements.Add (elem);
			}
		}

		public IList AnyAttributes {
			get { return _anyAttribute; }
		}

		public IList AnyElements {
			get { return _anyElement; }
		}
	}

}
