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
		{
			XmlValidatingReader vr = new XmlValidatingReader (new XmlTextReader (stream));
			vr.ValidationType = ValidationType.None;
			Initialize (vr, XmlSpace.None);
		}

		public XPathDocument (string uri) 
			: this (uri, XmlSpace.None)
		{
		}

		public XPathDocument (TextReader reader)
		{
			XmlValidatingReader vr = new XmlValidatingReader (new XmlTextReader (reader));
			vr.ValidationType = ValidationType.None;
			Initialize (vr, XmlSpace.None);
		}

		public XPathDocument (XmlReader reader)
			: this (reader, XmlSpace.None)
		{
		}

		public XPathDocument (string uri, XmlSpace space)
		{
			XmlValidatingReader vr = new XmlValidatingReader (new XmlTextReader (uri));
			vr.ValidationType = ValidationType.None;
			Initialize (vr, space);
		}

		public XPathDocument (XmlReader reader, XmlSpace space)
		{
			Initialize (reader, space);
		}

		private void Initialize (XmlReader reader, XmlSpace space)
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


