//
// XPathEditor.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C)2003 Atsushi Enomoto
//
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
	public abstract class XPathEditor : XPathNavigator2, IXPathEditor
	{
		protected XPathEditor ()
		{
		}

		public abstract XPathNavigator2 CloneAsNavigator ();

		public abstract XmlWriter CreateAttributes ();

		// TODO: Where to use schemaType?
		public void CreateAttributeString (string prefix, string name, string ns, XmlSchemaType schemaType, string value)
		{
			XmlWriter xw = CreateAttributes ();
			xw.WriteAttributeString (prefix, name, ns, value);
			xw.Close ();
		}

		public abstract XmlWriter CreateFirstChild ();

		public abstract void CreateFirstChild (string xmlFragments);

		// TODO: Where to use schemaType?
		public void CreateFirstChildElement (string prefix, string name, string ns, XmlSchemaType schemaType, string value)
		{
			XmlWriter xw = CreateFirstChild ();
			xw.WriteStartElement (prefix, name, ns);
			xw.WriteString (value);
			xw.WriteEndElement ();
			xw.Close ();
		}

		public abstract XmlWriter CreateNextSibling ();
		public abstract void CreateNextSibling (string xmlFragment);

		// TODO: Where to use schemaType?
		public void CreateNextSiblingElement (string prefix, string name, string ns, XmlSchemaType schemaType, string value)
		{
			XmlWriter xw = CreateNextSibling ();
			xw.WriteStartElement (prefix, name, ns);
			xw.WriteString (value);
			xw.WriteEndElement ();
			xw.Close ();
		}

		public abstract void DeleteCurrent ();

		public abstract void SetValue (string text);
	}
}
