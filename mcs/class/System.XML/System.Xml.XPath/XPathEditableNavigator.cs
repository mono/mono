//
// XPathEditableNavigator.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C)2003 Atsushi Enomoto
//

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Security.Policy;
using System.Xml.Schema;
using System.Xml.XPath;
//using Mono.Xml.XPath2;
//using MS.Internal.Xml;

namespace System.Xml
{
	public abstract class XPathEditableNavigator 
		: XPathNavigator, IXPathEditable
	{
		protected XPathEditableNavigator ()
		{
		}

		public abstract XmlWriter AppendChild ();

		public virtual XPathEditableNavigator AppendChild (
			string xmlFragments)
		{
			// FIXME: should XmlParserContext be something?
			return AppendChild (new XmlTextReader (xmlFragments, XmlNodeType.Element, null));
		}

		[MonoTODO]
		public virtual XPathEditableNavigator AppendChild (
			XmlReader reader)
		{
			XmlWriter w = AppendChild ();
			w.WriteNode (reader, false);
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XPathEditableNavigator AppendChild (
			XPathNavigator nav)
		{
			throw new NotImplementedException ();
//			AppendChild (new XPathNavigatorReader (nav));
		}

		public void AppendChildElement (string prefix, string name, string ns, string value)
		{
			XmlWriter xw = AppendChild ();
			xw.WriteStartElement (prefix, name, ns);
			xw.WriteString (value);
			xw.WriteEndElement ();
			xw.Close ();
		}

		public virtual void CreateAttribute (string prefix, string localName, string namespaceURI, string value)
		{
			using (XmlWriter w = CreateAttributes ()) {
				w.WriteAttributeString (prefix, localName, namespaceURI, value);
			}
		}

		public abstract XmlWriter CreateAttributes ();

		public virtual XPathEditableNavigator CreateEditor ()
		{
			return (XPathEditableNavigator) Clone ();
		}

		// LAMESPEC: documented as public abstract, but it conflicts
		// with XPathNavigator.CreateNavigator ().
//		public abstract XPathNavigator CreateNavigator ();

		public abstract bool DeleteCurrent ();

		public abstract XmlWriter InsertAfter ();

		public virtual XPathEditableNavigator InsertAfter (string xmlFragments)
		{
			return InsertAfter (new XmlTextReader (xmlFragments, XmlNodeType.Element, null));
		}

		[MonoTODO]
		public virtual XPathEditableNavigator InsertAfter (XmlReader reader)
		{
			using (XmlWriter w = InsertAfter ()) {
				w.WriteNode (reader, false);
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XPathEditableNavigator InsertAfter (XPathNavigator nav)
		{
//			InsertAfter (new XPathNavigatorReader (nav));
			throw new NotImplementedException ();
		}

		public abstract XmlWriter InsertBefore ();

		public virtual XPathEditableNavigator InsertBefore (string xmlFragments)
		{
			return InsertBefore (new XmlTextReader (xmlFragments, XmlNodeType.Element, null));
		}

		[MonoTODO]
		public virtual XPathEditableNavigator InsertBefore (XmlReader reader)
		{
			using (XmlWriter w = InsertBefore ()) {
				w.WriteNode (reader, false);
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XPathEditableNavigator InsertBefore (XPathNavigator nav)
		{
//			InsertBefore (new XPathNavigatorReader (nav));
			throw new NotImplementedException ();
		}

		public virtual void InsertElementAfter (string prefix, 
			string localName, string namespaceURI, string value)
		{
			using (XmlWriter w = InsertAfter ()) {
				w.WriteElementString (prefix, localName, namespaceURI, value);
			}
		}

		public virtual void InsertElementBefore (string prefix, 
			string localName, string namespaceURI, string value)
		{
			using (XmlWriter w = InsertBefore ()) {
				w.WriteElementString (prefix, localName, namespaceURI, value);
			}
		}

		public abstract XmlWriter PrependChild ();

		public virtual XPathEditableNavigator PrependChild (string xmlFragments)
		{
			return PrependChild (new XmlTextReader (xmlFragments, XmlNodeType.Element, null));
		}

		[MonoTODO]
		public virtual XPathEditableNavigator PrependChild (XmlReader reader)
		{
			using (XmlWriter w = PrependChild ()) {
				w.WriteNode (reader, false);
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XPathEditableNavigator PrependChild (XPathNavigator nav)
		{
//			PrependChild (new XPathNavigatorReader (nav));
			throw new NotImplementedException ();
		}

		public virtual void PrependChildElement (string prefix, 
			string localName, string namespaceURI, string value)
		{
			using (XmlWriter w = PrependChild ()) {
				w.WriteElementString (prefix, localName, namespaceURI, value);
			}
		}

		// Dunno the exact purpose, but maybe internal editor use
		[MonoTODO]
		public virtual void SetFromObject (object value)
		{
			throw new NotImplementedException ();
		}

		public abstract void SetValue (object value);

		[MonoTODO]
		public virtual void Validate (XmlSchemaSet schemas, ValidationEventHandler handler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Validate (XmlSchemaSet schemas, ValidationEventHandler handler, XmlSchemaAttribute attribute)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Validate (XmlSchemaSet schemas, ValidationEventHandler handler, XmlSchemaElement element)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Validate (XmlSchemaSet schemas, ValidationEventHandler handler, XmlSchemaType schemaType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string InnerXml {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override string OuterXml {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}

#endif
