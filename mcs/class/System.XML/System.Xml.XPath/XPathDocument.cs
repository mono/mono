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

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Mono.Xml.XPath;

//using InternalBuilder = Mono.Xml.XPath.DTMXPathDocumentBuilder;
//using InternalDocument = Mono.Xml.XPath.DTMXPathDocument;
using InternalBuilder = Mono.Xml.XPath.DTMXPathDocumentBuilder2;
using InternalDocument = Mono.Xml.XPath.DTMXPathDocument2;

namespace System.Xml.XPath
{
	public class XPathDocument : IXPathNavigable
	{
		IXPathNavigable document;

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

		public XPathDocument (TextReader textReader)
		{
			XmlValidatingReader vr = new XmlValidatingReader (new XmlTextReader (textReader));
			vr.ValidationType = ValidationType.None;
			Initialize (vr, XmlSpace.None);
		}

		public XPathDocument (XmlReader reader)
			: this (reader, XmlSpace.None)
		{
		}

		public XPathDocument (string uri, XmlSpace space)
		{
			XmlValidatingReader vr = null;
			try {
				vr = new XmlValidatingReader (new XmlTextReader (uri));
				vr.ValidationType = ValidationType.None;
				Initialize (vr, space);
			} finally {
				if (vr != null)
					vr.Close ();
			}
		}

		public XPathDocument (XmlReader reader, XmlSpace space)
		{
			Initialize (reader, space);
		}

		private void Initialize (XmlReader reader, XmlSpace space)
		{
			document = new InternalBuilder (reader, space).CreateDocument ();
		}

		public XPathNavigator CreateNavigator ()
		{
			return document.CreateNavigator ();
		}
	}
}


