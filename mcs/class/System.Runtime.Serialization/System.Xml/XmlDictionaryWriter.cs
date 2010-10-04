//
// XmlDictionaryWriter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
#if NET_2_0
using System;
using System.IO;
using System.Text;

namespace System.Xml
{
	public abstract partial class XmlDictionaryWriter : XmlWriter
	{
		static readonly Encoding utf8_unmarked = new UTF8Encoding (false);

		protected XmlDictionaryWriter ()
		{
		}

		internal int Depth { get; set; }

		internal int NSIndex { get; set; }

		public virtual bool CanCanonicalize {
			get { return false; }
		}

		public static XmlDictionaryWriter CreateBinaryWriter (
			Stream stream)
		{
			return CreateBinaryWriter (stream, null, null, false);
		}

		public static XmlDictionaryWriter CreateBinaryWriter (
			Stream stream, IXmlDictionary dictionary)
		{
			return CreateBinaryWriter (stream, dictionary, null, false);
		}

		public static XmlDictionaryWriter CreateBinaryWriter (
			Stream stream, IXmlDictionary dictionary,
			XmlBinaryWriterSession session)
		{
			return CreateBinaryWriter (stream, dictionary, session, false);
		}

		public static XmlDictionaryWriter CreateBinaryWriter (
			Stream stream, IXmlDictionary dictionary,
			XmlBinaryWriterSession session, bool ownsStream)
		{
			return new XmlBinaryDictionaryWriter (stream,
				dictionary, session, ownsStream);
		}

		public static XmlDictionaryWriter CreateDictionaryWriter (XmlWriter writer)
		{
			return new XmlSimpleDictionaryWriter (writer);
		}
#if !NET_2_1
		public static XmlDictionaryWriter CreateMtomWriter (
			Stream stream, Encoding encoding, int maxSizeInBytes,
			string startInfo)
		{
			return CreateMtomWriter (stream, encoding,
				maxSizeInBytes, startInfo, Guid.NewGuid () + "id=1", "http://tempuri.org/0/" + DateTime.Now.Ticks, true, false);
		}

		public static XmlDictionaryWriter CreateMtomWriter (
			Stream stream, Encoding encoding, int maxSizeInBytes,
			string startInfo, string boundary, string startUri,
			bool writeMessageHeaders, bool ownsStream)
		{
			return new XmlMtomDictionaryWriter (stream, encoding, maxSizeInBytes, startInfo, boundary, startUri, writeMessageHeaders, ownsStream);
		}
#endif
		public static XmlDictionaryWriter CreateTextWriter (
			Stream stream)
		{
			return CreateTextWriter (stream, Encoding.UTF8);
		}

		public static XmlDictionaryWriter CreateTextWriter (
			Stream stream, Encoding encoding)
		{
			return CreateTextWriter (stream, encoding, false);
		}

		// BTW looks like it creates an instance of different
		// implementation than those from XmlWriter.Create().
		public static XmlDictionaryWriter CreateTextWriter (
			Stream stream, Encoding encoding, bool ownsStream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			if (encoding == null)
				throw new ArgumentNullException ("encoding");

			switch (encoding.CodePage) {
			case 1200:
			case 1201: // utf-16
			case 65001: // utf-8
				encoding = utf8_unmarked;
				break;
			default:
				throw new XmlException (String.Format ("XML declaration is required for encoding code page {0} but this XmlWriter does not support XML declaration.", encoding.CodePage));
			}

			XmlWriterSettings s = new XmlWriterSettings ();
			s.Encoding = encoding;
			s.CloseOutput = ownsStream;
			s.OmitXmlDeclaration = true;
			return CreateDictionaryWriter (XmlWriter.Create (stream, s));
		}

		

		public virtual void EndCanonicalization ()
		{
			throw new NotSupportedException ();
		}

		public virtual void StartCanonicalization (
			Stream stream, bool includeComments,
			string [] inclusivePrefixes)
		{
			throw new NotSupportedException ();
		}

		public void WriteAttributeString (
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri,
			string value)
		{
			WriteAttributeString (null, localName, namespaceUri, value);
		}

		public void WriteAttributeString (string prefix,
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri,
			string value)
		{
			WriteStartAttribute (prefix, localName, namespaceUri);
			WriteString (value);
			WriteEndAttribute ();
		}

		public void WriteElementString (
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri,
			string value)
		{
			WriteElementString (null, localName, namespaceUri, value);
		}

		public void WriteElementString (string prefix,
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri,
			string value)
		{
			WriteStartElement (prefix, localName, namespaceUri);
			WriteString (value);
			WriteEndElement ();
		}

		public virtual void WriteNode (XmlDictionaryReader reader,
			bool defattr)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			switch (reader.NodeType) {
			case XmlNodeType.Element:
				// gratuitously copied from System.XML/System.Xml/XmlWriter.cs:WriteNode(XmlReader,bool)
				// as there doesn't seem to be a way to hook into attribute writing w/o handling Element.
				XmlDictionaryString ename, ens;
				if (reader.TryGetLocalNameAsDictionaryString (out ename) && reader.TryGetLocalNameAsDictionaryString (out ens))
					WriteStartElement (reader.Prefix, ename, ens);
				else
					WriteStartElement (reader.Prefix, reader.LocalName, reader.NamespaceURI);
				// Well, I found that MS.NET took this way, since
				// there was a error-prone SgmlReader that fails
				// MoveToNextAttribute().
				if (reader.HasAttributes) {
					for (int i = 0; i < reader.AttributeCount; i++) {
						reader.MoveToAttribute (i);
						WriteAttribute (reader, defattr);
					}
					reader.MoveToElement ();
				}
				if (reader.IsEmptyElement)
					WriteEndElement ();
				else {
					int depth = reader.Depth;
					reader.Read ();
					if (reader.NodeType != XmlNodeType.EndElement) {
						do {
							WriteNode (reader, defattr);
						} while (depth < reader.Depth);
					}
					WriteFullEndElement ();
				}
				reader.Read ();
				break;
			case XmlNodeType.Attribute:
			case XmlNodeType.Text:
				WriteTextNode (reader, defattr);
				break;
			default:
				base.WriteNode (reader, defattr);
				break;
			}
		}

		private void WriteAttribute (XmlDictionaryReader reader, bool defattr)
		{
			if (!defattr && reader.IsDefault)
				return;

			XmlDictionaryString name, ns;
			if (reader.TryGetLocalNameAsDictionaryString (out name) && reader.TryGetLocalNameAsDictionaryString (out ns))
				WriteStartAttribute (reader.Prefix, name, ns);
			else
				WriteStartAttribute (reader.Prefix, reader.LocalName, reader.NamespaceURI);
#if NET_2_1
			// no ReadAttributeValue() in 2.1 profile.
			WriteTextNode (reader, true);
#else
			while (reader.ReadAttributeValue ()) {
				switch (reader.NodeType) {
				case XmlNodeType.Text:
					WriteTextNode (reader, true);
					break;
				case XmlNodeType.EntityReference:
					WriteEntityRef (reader.Name);
					break;
				}
			}
#endif
			WriteEndAttribute ();
		}

		public override void WriteNode (XmlReader reader, bool defattr)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			XmlDictionaryReader dr = reader as XmlDictionaryReader;
			if (dr != null)
				WriteNode (dr, defattr);
			else
				base.WriteNode (reader, defattr);
		}

		public virtual void WriteQualifiedName (
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri)
		{
			WriteQualifiedName (localName.Value, namespaceUri.Value);
		}

		public void WriteStartAttribute (
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri)
		{
			WriteStartAttribute (localName.Value, namespaceUri.Value);
		}

		public virtual void WriteStartAttribute (string prefix,
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri)
		{
			WriteStartAttribute (prefix, localName.Value, namespaceUri.Value);
		}

		public void WriteStartElement (
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri)
		{
			WriteStartElement (null, localName, namespaceUri);
		}

		public virtual void WriteStartElement (string prefix,
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri)
		{
			if (localName == null)
				throw new ArgumentException ("localName must not be null.", "localName");
			WriteStartElement (prefix, localName.Value,
					namespaceUri != null ? namespaceUri.Value : null);
		}

		public virtual void WriteString (XmlDictionaryString value)
		{
			WriteString (value.Value);
		}

		protected virtual void WriteTextNode (XmlDictionaryReader reader, bool isAttribute)
		{
			WriteString (reader.Value);
			if (!isAttribute)
				reader.Read ();
		}

		public virtual void WriteValue (Guid guid)
		{
			WriteString (guid.ToString ());
		}

		public virtual void WriteValue (IStreamProvider value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			Stream stream = value.GetStream ();
			byte[] buf = new byte [Math.Min (2048, stream.CanSeek ? stream.Length : 2048)];
			int read;
			while ((read = stream.Read (buf, 0, buf.Length)) > 0) {
				WriteBase64 (buf, 0, read);
			}
			value.ReleaseStream (stream);
		}

		public virtual void WriteValue (TimeSpan duration)
		{
			WriteString (XmlConvert.ToString (duration));
		}

		public virtual void WriteValue (UniqueId id)
		{
			if (id == null)
				throw new ArgumentNullException ("id");
			WriteString (id.ToString ());
		}

		public virtual void WriteValue (XmlDictionaryString value)
		{
			WriteValue (value.Value);
		}

		public virtual void WriteXmlAttribute (string localName, string value)
		{
			WriteAttributeString ("xml", localName, "http://www.w3.org/XML/1998/namespace", value);
		}

		public virtual void WriteXmlAttribute (XmlDictionaryString localName,
			XmlDictionaryString value)
		{
			WriteXmlAttribute (localName.Value, value.Value);
		}

		public virtual void WriteXmlnsAttribute (
			string prefix, string namespaceUri)
		{
			// BTW .NET 2.0 those XmlWriters from XmlWrite.Create()
			// rejects namespace overriding i.e.
			//
			//	xw.WriteStartElement ("foo", "urn:foo");
			//	xw.WriteXmlnsAttribute ("foo", "urn:bar");
			//
			// causes an XmlException. We need fix in sys.xml.dll

			// When the prefix is null, this writer must mock
			// a dummy namespace up. It is then up to the actual
			// writer how it is determined in the output. (When
			// there is a duplicate, then it will be further 
			// modified.)
			if (prefix == null && String.IsNullOrEmpty (namespaceUri))
				prefix = String.Empty;
			else if (prefix == null)
				prefix = "d" + Depth + "p" + (++NSIndex);

			if (prefix == String.Empty)
				WriteAttributeString ("xmlns", namespaceUri);
			else
				WriteAttributeString ("xmlns", prefix, "http://www.w3.org/2000/xmlns/", namespaceUri);
		}

		public virtual void WriteXmlnsAttribute (string prefix,
			XmlDictionaryString namespaceUri)
		{
			WriteXmlnsAttribute (prefix, namespaceUri.Value);
		}
	}
}
#endif
