//
// Microsoft.Web.Services.Xml
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using System.Collections;

namespace Microsoft.Web.Services.Xml
{

	public abstract class OpenAttributeElement
	{
		
		private ArrayList _any;
		
		public OpenAttributeElement ()
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

			foreach(XmlAttribute attrib in AnyAttributes) {
				element.Attributes.Append((XmlAttribute)document.ImportNode(attrib, true));
			}
		}

		public void LoadXmlAny (XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}
			foreach(XmlAttribute attrib in element.Attributes) {
				AnyAttributes.Add(attrib);
			}
		}

		public IList AnyAttributes {
			get { return _any; }
		}
	
	}
	
}
