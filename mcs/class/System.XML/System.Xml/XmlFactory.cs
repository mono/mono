//
// XmlFactory.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
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
