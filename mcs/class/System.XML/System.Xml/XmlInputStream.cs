//
// System.Xml.XmlInputStream 
//	encoding-specification-wise XML input stream and reader
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
//
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Mono.Xml.Native
{
	#region XmlStreamReader
	public class XmlStreamReader : StreamReader
	{
		XmlInputStream input;

		XmlStreamReader (XmlInputStream input)
			: base (input, input.ActualEncoding != null ? input.ActualEncoding : Encoding.UTF8)
		{
			this.input = input;
		}

		public XmlStreamReader (Stream input)
			: this (new XmlInputStream (input, true))
		{
		}

		public XmlStreamReader (Stream input, bool docent)
			: this (new XmlInputStream (input, docent))
		{
		}

//		public XmlStreamReader (string url)
//			: this (url, true)
//		{
//		}
//
//		public XmlStreamReader (string url, bool docent)
//			: this (new XmlInputStream (url, docent, null, null))
//		{
//		}

		public XmlStreamReader (string url, XmlResolver resolver, string baseURI)
			: this (url, true, resolver, baseURI)
		{
		}

		public XmlStreamReader (string url, bool docent, XmlResolver resolver,
			string baseURI)
			: this (new XmlInputStream (url, docent, resolver, baseURI))
		{
		}

		public override void Close ()
		{
			this.input.Close ();
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (disposing) {
				Close ();
			}
		}

	}
	#endregion

	class XmlInputStream : Stream
	{
		Encoding enc;
		Stream stream;
		byte[] buffer = new byte[256];
		int bufLength;
		int bufPos;
		bool isDocumentEntity;	// allow omitting "version" or not.

		static XmlException encodingException = new XmlException ("invalid encoding specification.");
/*
		public XmlInputStream (string url)
			: this (url, true)
		{
		}
*/
		public XmlInputStream (string url, bool docent, XmlResolver resolver, string baseURI)
		{
			this.isDocumentEntity = docent;
			// Use XmlResolver to resolve external entity.

			if (resolver == null)
				resolver = new XmlUrlResolver ();
			Uri uri = resolver.ResolveUri (
				baseURI == null || baseURI == String.Empty ?
                                null : new Uri (baseURI), url);
			Stream s = resolver.GetEntity (uri, null, typeof (Stream)) as Stream;

			Initialize (s);
		}

		public XmlInputStream (Stream stream)
			: this (stream, true)
		{
		}

		public XmlInputStream (Stream stream, bool docent)
		{
			this.isDocumentEntity = docent;
			Initialize (stream);
		}

		private void Initialize (Stream stream)
		{
			// FIXME: seems too waste...
			MemoryStream ms = new MemoryStream ();
			this.stream = stream;
			int c = stream.ReadByte ();
			switch (c) {
			case 0xFF:
				c = stream.ReadByte ();
				if (c == 0xFE) {
					// BOM-ed little endian utf-16
					enc = Encoding.Unicode;
				} else {
					// It doesn't start from "<?xml" then its encoding is utf-8
					enc = Encoding.UTF8;
					ms.WriteByte ((byte)0xFF);
					ms.WriteByte ((byte)c);
				}
				break;
			case 0xFE:
				c = stream.ReadByte ();
				if (c == 0xFF) {
					// BOM-ed big endian utf-16
					enc = Encoding.BigEndianUnicode;
					return;
				} else {
					// It doesn't start from "<?xml" then its encoding is utf-8
					enc = Encoding.UTF8;
					ms.WriteByte ((byte)0xFE);
					ms.WriteByte ((byte)c);
				}
				break;
			case 0xEF:
				enc = Encoding.UTF8;
				c = ReadByte ();
				if (c == 0xBB) {
					c = ReadByte ();
					if (c != 0xBF) {
						ms.WriteByte ((byte)0xEF);
						ms.WriteByte ((byte)0xBB);
						ms.WriteByte ((byte)c);
					}
				} else {
					ms.WriteByte ((byte)0xEF);
				}
				break;
			case '<':
				// try to get encoding name from XMLDecl.
				ms.WriteByte ((byte)'<');
				int size = stream.Read (buffer, 1, 4);
				ms.Write (buffer, 1, size);
				if (Encoding.ASCII.GetString (buffer, 1, size) == "?xml") {
					int loop = 0;
					c = SkipWhitespace (ms);

					// version. It is optional here.
					if (c != 'v') {
						// FIXME: temporarily comment out here.
//						if (isDocumentEntity)
//							throw new XmlException ("invalid xml declaration.");
					} else {
						ms.WriteByte ((byte)'v');
						while (loop++ >= 0 && c >= 0) {
							c = stream.ReadByte ();
							ms.WriteByte ((byte)c);
							if (c == '0') { // 0 of 1.0
								ms.WriteByte ((byte)stream.ReadByte ());
								break;
							}
						}
						c = SkipWhitespace (ms);
					}

					if (c == 'e') {
						ms.WriteByte ((byte)'e');
						size = stream.Read (buffer, 0, 7);
						ms.Write (buffer, 0, 7);
						if (Encoding.ASCII.GetString(buffer, 0, 7) == "ncoding") {
							c = this.SkipWhitespace(ms);
							if (c != '=')
								throw encodingException;
							ms.WriteByte ((byte)'=');
							c = this.SkipWhitespace (ms);
							int quoteChar = c;
							ms.WriteByte ((byte)c);
							int start = (int)ms.Position;
							while (loop++ >= 0) {
								c = stream.ReadByte ();
								if (c == quoteChar)
									break;
								else if (c < 0)
									throw encodingException;
								ms.WriteByte ((byte)c);
							}
							string encodingName = Encoding.UTF8.GetString (ms.GetBuffer (), start, (int)ms.Position - start);
							if (!XmlChar.IsValidIANAEncoding (encodingName))
								throw encodingException;
							ms.WriteByte ((byte)quoteChar);
							enc = Encoding.GetEncoding (encodingName);
						}
						else
							ms.Write (buffer, 0, size);
					}
					else
						ms.WriteByte ((byte)c);
				}
				buffer = ms.ToArray ();
				bufLength = buffer.Length;
				bufPos = 0;
				break;
			default:
				buffer [0] = (byte)c;
				bufLength = 1;
				enc = Encoding.UTF8;
				break;
			}
		}

		// skips whitespace and returns misc char that was read from stream
		private int SkipWhitespace (MemoryStream ms)	// ms may be null
		{
			int loop = 0;
			int c;
			while (loop++ >= 0) { // defends infinite loop (expecting overflow)
				c = stream.ReadByte ();
				switch (c) {
				case '\r': goto case ' ';
				case '\n': goto case ' ';
				case '\t': goto case ' ';
				case ' ':
					if (ms != null)
						ms.WriteByte ((byte)c);
					continue;
				default:
					return c;
				}
			}
			throw new InvalidOperationException ();
		}

		public Encoding ActualEncoding {
			get { return enc; }
		}

		#region Public Overrides
		public override bool CanRead {
			get {
				if (bufLength > bufPos)
					return true;
				else
					return stream.CanRead; 
			}
		}

		// FIXME: It should support base stream's CanSeek.
		public override bool CanSeek {
			get { return false; } // stream.CanSeek; }
		}

		public override bool CanWrite {
			get { return false; }
		}

		public override long Length {
			get {
				return stream.Length;
			}
		}

		public override long Position {
			get {
				return stream.Position - bufLength + bufPos;
			}
			set {
				if(value < bufLength)
					bufPos = (int)value;
				else
					stream.Position = value - bufLength;
			}
		}

		public override void Close ()
		{
			stream.Close ();
		}

		public override void Flush ()
		{
			stream.Flush ();
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			int ret;
			if (count <= bufLength - bufPos)	{	// all from buffer
				Array.Copy (this.buffer, bufPos, buffer, offset, count);
				bufPos += count;
				ret = count;
			} else {
				int bufRest = bufLength - bufPos;
				if (bufLength > bufPos) {
					Array.Copy (this.buffer, bufPos, buffer, offset, bufRest);
					bufPos += bufRest;
				}
				ret = bufRest +
					stream.Read (buffer, offset + bufRest, count - bufRest);
			}
			return ret;
		}

		public override int ReadByte ()
		{
			if (bufLength > bufPos) {
				return buffer [bufPos++];
			}
			return stream.ReadByte ();
		}

		public override long Seek (long offset, System.IO.SeekOrigin origin)
		{
			int bufRest = bufLength - bufPos;
			if (origin == SeekOrigin.Current)
				if (offset < bufRest)
					return buffer [bufPos + offset];
				else
					return stream.Seek (offset - bufRest, origin);
			else
				return stream.Seek (offset, origin);
		}

		public override void SetLength (long value)
		{
			stream.SetLength (value);
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException ();
		}
		#endregion
	}
}
