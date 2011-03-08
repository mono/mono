//
// XmlSyndicationContent.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace System.ServiceModel.Syndication
{
	public class XmlSyndicationContent : SyndicationContent
	{
		SyndicationElementExtension extension;
		string type;

		public XmlSyndicationContent (XmlReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			this.type = reader.GetAttribute ("type");
			extension = new SyndicationElementExtension (reader);
		}

		public XmlSyndicationContent (string type, object dataContractExtension, XmlObjectSerializer dataContractSerializer)
		{
			this.type = type;
			extension = new SyndicationElementExtension (dataContractExtension, dataContractSerializer);
		}

		public XmlSyndicationContent (string type, object xmlSerializerExtension, XmlSerializer serializer)
		{
			this.type = type;
			extension = new SyndicationElementExtension (xmlSerializerExtension, serializer);
		}

		public XmlSyndicationContent (string type, SyndicationElementExtension extension)
		{
			this.type = type;
			if (extension == null)
				throw new ArgumentNullException ("extension");
			this.extension = extension;
		}

		protected XmlSyndicationContent (XmlSyndicationContent source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			type = source.type;
			extension = source.extension;
		}

		public override SyndicationContent Clone ()
		{
			return new XmlSyndicationContent (this);
		}

		public XmlDictionaryReader GetReaderAtContent ()
		{
			var ms = new MemoryStream ();
			using (var bw = XmlDictionaryWriter.CreateBinaryWriter (ms)) {
				// default seems to be Atom 1.0
				bw.WriteStartElement ("content", Namespaces.Atom10);
				bw.WriteAttributeString ("type", "text/xml");

				XmlReader r = extension.GetReader ();
				while (!r.EOF)
					bw.WriteNode (r, false);

				bw.WriteEndElement (); // </content>
			}
			ms.Position = 0;
			var ret = XmlDictionaryReader.CreateBinaryReader (ms, new XmlDictionaryReaderQuotas ());
			ret.MoveToContent ();
			return ret;
		}

		public TContent ReadContent<TContent> ()
		{
			return extension.GetObject<TContent> ();
		}

		public TContent ReadContent<TContent> (XmlObjectSerializer serializer)
		{
			return extension.GetObject<TContent> (serializer);
		}

		public TContent ReadContent<TContent> (XmlSerializer serializer)
		{
			return extension.GetObject<TContent> (serializer);
		}

		protected override void WriteContentsTo (XmlWriter writer)
		{
			extension.WriteTo (writer);
		}

		public SyndicationElementExtension Extension {
			get { return extension; }
		}

		public override string Type {
			get { return type ?? "text/xml"; }
		}
	}
}
