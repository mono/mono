//
// XmlFactory.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
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

#if NET_2_0

using System;
using System.IO;
using System.Text;

namespace System.Xml
{
	public class XmlFactory
	{
		XmlNameTable nameTable;
		XmlReaderSettings readerSettings;
		XmlWriterSettings writerSettings;

		public XmlFactory ()
		{
		}

		// CreateReader

		[MonoTODO]
		public XmlReader CreateReader (string url)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlReader CreateReader (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlReader CreateReader (Stream stream, string baseUri)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlReader CreateReader (string url, XmlResolver resolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlReader CreateReader (TextReader reader, XmlResolver resolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlReader CreateReader (TextReader reader, string baseUri)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlReader CreateReader (XmlReader reader, XmlResolver resolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlReader CreateReader (Stream stream, string baseUri, Encoding encoding)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlReader CreateReader (Stream stream, string baseUri, XmlResolver resolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlReader CreateReader (TextReader reader, string baseUri, Encoding encoding)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlReader CreateReader (Stream stream, string baseUri, Encoding encoding, XmlResolver resolver)
		{
			throw new NotImplementedException ();
		}

		// CreateWriter

		[MonoTODO]
		public XmlWriter CreateWriter (Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlWriter CreateWriter (string url)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlWriter CreateWriter (StringBuilder builder)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlWriter CreateWriter (TextWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlWriter CreateWriter (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlWriter CreateWriter (StringBuilder builder, bool indent)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlWriter CreateWriter (TextWriter writer, bool indent)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlWriter CreateWriter (Stream stream, Encoding encoding, bool indent)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlWriter CreateWriter (string url, Encoding encoding, bool indent)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("avoid null")]
		public XmlNameTable NameTable {
			get { return nameTable; }
			set { nameTable = value; }
		}

		[MonoTODO ("avoid null")]
		public XmlReaderSettings ReaderSettings {
			get { return readerSettings; }
			set { readerSettings = value; }
		}

		[MonoTODO ("avoid null")]
		public XmlWriterSettings WriterSettings {
			get { return writerSettings; }
			set { writerSettings = value; }
		}
	}
}

#endif
