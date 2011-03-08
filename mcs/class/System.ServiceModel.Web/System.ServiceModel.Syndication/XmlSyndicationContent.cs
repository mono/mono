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
		SyndicationElementExtension writer_extension, reader_extension;
		string type;

		public XmlSyndicationContent (XmlReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			this.type = reader.GetAttribute ("type");
			reader_extension = new SyndicationElementExtension (reader);
		}

		public XmlSyndicationContent (string type, object dataContractExtension, XmlObjectSerializer dataContractSerializer)
		{
			this.type = type;
			writer_extension = new SyndicationElementExtension (dataContractExtension, dataContractSerializer);
		}

		public XmlSyndicationContent (string type, object xmlSerializerExtension, XmlSerializer serializer)
		{
			this.type = type;
			writer_extension = new SyndicationElementExtension (xmlSerializerExtension, serializer);
		}

		public XmlSyndicationContent (string type, SyndicationElementExtension extension)
		{
			this.type = type;
			if (extension == null)
				throw new ArgumentNullException ("extension");
			this.writer_extension = extension;
		}

		protected XmlSyndicationContent (XmlSyndicationContent source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			type = source.type;
			writer_extension = source.writer_extension;
			reader_extension = source.reader_extension;
		}

		public override SyndicationContent Clone ()
		{
			return new XmlSyndicationContent (this);
		}

		SyndicationElementExtension extension {
			get { return writer_extension ?? reader_extension; }
		}

		public XmlDictionaryReader GetReaderAtContent ()
		{
			if (writer_extension != null) {
				// It is messy, but it somehow returns an XmlReader that has wrapper "content" element for non-XmlReader extension...
				XmlReader r = extension.GetReader ();
				if (!(r is XmlDictionaryReader))
					r = XmlDictionaryReader.CreateDictionaryReader (r);
				var ms = new MemoryStream ();
				var xw = XmlDictionaryWriter.CreateBinaryWriter (ms);
				xw.WriteStartElement ("content", Namespaces.Atom10);
				xw.WriteAttributeString ("type", "text/xml");
				while (!r.EOF)
					xw.WriteNode (r, false);
				xw.WriteEndElement ();
				xw.Close ();
				ms.Position = 0;
				var xr = XmlDictionaryReader.CreateBinaryReader (ms, new XmlDictionaryReaderQuotas ());
				xr.MoveToContent ();
				return xr;
			} else {
				XmlReader r = extension.GetReader ();
				if (!(r is XmlDictionaryReader))
					r = XmlDictionaryReader.CreateDictionaryReader (r);
				return (XmlDictionaryReader) r;
			}
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
			if (reader_extension != null) {
				// It is messy, but it somehow skips the wrapper element...
				var xr = extension.GetReader ();
				if (xr.IsEmptyElement)
					xr.Read ();
				else {
					xr.ReadStartElement (); // skip it
					while (xr.NodeType != XmlNodeType.EndElement) {
						writer.WriteNode (xr, false);
					}
					xr.ReadEndElement ();
				}
			}
			else
				extension.WriteTo (writer);
		}

		public SyndicationElementExtension Extension {
			get { return writer_extension; }
		}

		public override string Type {
			get { return type ?? "text/xml"; }
		}
	}
}
