//
// System.Xml.XPath.XPathDocument
//
// Authors:
//   Tim Coleman (tim@timcoleman.com)
//   Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) Copyright 2002 Tim Coleman
// (C) 2003 Atsushi Enomoto
//
using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using Mono.Xml.XPath;

namespace System.Xml.XPath
{

	public class XPathDocument : IXPathNavigable
	{
		DTMXPathDocument document;

#region Constructors

		public XPathDocument (Stream stream)
			: this (new XmlTextReader (stream))
		{
		}

		public XPathDocument (string uri)
			: this (new XmlTextReader (uri))
		{
		}

		public XPathDocument (TextReader reader)
			: this (new XmlTextReader (reader))
		{
		}

		public XPathDocument (XmlReader reader)
			: this (reader, XmlSpace.None)
		{
		}

		public XPathDocument (string uri, XmlSpace space)
			: this (new XmlTextReader (uri), space)
		{
		}

		public XPathDocument (XmlReader reader, XmlSpace space)
		{
			document = new DTMXPathDocumentBuilder (reader, space).CreateDocument ();
		}

#endregion

#region Methods

		public XPathNavigator CreateNavigator ()
		{
			return document.CreateNavigator ();
		}

#endregion

	}

}


