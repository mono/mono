//
// Microsoft.Web.Services.Xml.OpenElementElement.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using System.Collections;

namespace Microsoft.Web.Services.Xml
{
	public abstract class OpenElementElement
	{
		private ArrayList _any;

		public OpenElementElement ()
		{
			_any = new ArrayList ();
		}

		public void GetXmlAny (XmlDocument document, XmlElement element)
		{
			if(document == null) {
				throw new ArgumentNullException ("document");
			}
			if(element == null) {
				throw new ArgumentNullException ("element");
			}

			foreach(XmlElement elem in AnyElements) {
				element.AppendChild(document.ImportNode(elem, true));
			}

		}

		public void LoadXmlAny (XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}

			foreach(XmlElement elem in element.ChildNodes) {
				AnyElements.Add (elem);
			}
		}

		public ArrayList AnyElements {
			get { return _any; }
		}
	}
}
