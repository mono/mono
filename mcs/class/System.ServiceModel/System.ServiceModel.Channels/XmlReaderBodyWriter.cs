//
// XmlReaderBodyWriter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace System.ServiceModel.Channels
{
	internal class XmlReaderBodyWriter : BodyWriter
	{
		XmlDictionaryReader reader;
		string xml_bak;
		XmlParserContext parser_context;
		bool consumed;

		public XmlReaderBodyWriter (string xml, int maxBufferSize, XmlParserContext ctx)
			: base (true)
		{
			var settings = new XmlReaderSettings () {
				// FIXME: enable this line (once MaxCharactersInDocument is implemented)
				// MaxCharactersInDocument = maxBufferSize,
				ConformanceLevel = ConformanceLevel.Fragment
				};
			reader = XmlDictionaryReader.CreateDictionaryReader (XmlReader.Create (new StringReader (xml), settings, ctx));
			reader.MoveToContent ();
			xml_bak = xml;
			parser_context = ctx;
		}

		public XmlReaderBodyWriter (XmlDictionaryReader reader)
			: base (false)
		{
			reader.MoveToContent ();
			this.reader = reader;
		}

		protected override BodyWriter OnCreateBufferedCopy (
			int maxBufferSize)
		{
#if true
			if (xml_bak == null) {
				if (consumed)
					throw new InvalidOperationException ("Body xml reader is already consumed");
				var sw = new StringWriter ();
				var xw = XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (sw));
				xw.WriteStartElement (reader.Prefix, reader.LocalName, reader.NamespaceURI);
				xw.WriteAttributes (reader, false);

				var inr = reader as IXmlNamespaceResolver;
				if (inr != null)
					foreach (var p in inr.GetNamespacesInScope (XmlNamespaceScope.ExcludeXml))
						if (xw.LookupPrefix (p.Value) != p.Key)
							xw.WriteXmlnsAttribute (p.Key, p.Value);
				if (!reader.IsEmptyElement) {
					reader.Read ();
					while (reader.NodeType != XmlNodeType.EndElement)
						xw.WriteNode (reader, false);
				}
				xw.WriteEndElement ();

				xw.Close ();
				xml_bak = sw.ToString ();
				reader = null;
			}
#else // FIXME: this should be better, but somehow doesn't work.
			if (xml_bak == null) {
				if (consumed)
					throw new InvalidOperationException ("Body xml reader is already consumed");
				var nss = new XmlNamespaceManager (reader.NameTable);
				var nsr = reader as IXmlNamespaceResolver;
				if (nsr != null)
					foreach (var p in nsr.GetNamespacesInScope (XmlNamespaceScope.ExcludeXml))
						nss.AddNamespace (p.Key, p.Value);
				parser_context = new XmlParserContext (nss.NameTable, nss, reader.XmlLang, reader.XmlSpace);
				xml_bak = reader.ReadOuterXml ();
			}
#endif
			return new XmlReaderBodyWriter (xml_bak, maxBufferSize, parser_context);
		}

		protected override void OnWriteBodyContents (
			XmlDictionaryWriter writer)
		{
			if (consumed)
				throw new InvalidOperationException ("Body xml reader is already consumed");
			if (reader == null && String.IsNullOrEmpty (xml_bak))
				return;
			XmlReader r = xml_bak != null ? XmlReader.Create (new StringReader (xml_bak), null, parser_context) : reader;
			r.MoveToContent ();
			writer.WriteNode (r, false);
			if (xml_bak == null)
				consumed = true;
		}
	}
}
