//
// JsonReaderWriterFactory.cs
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
using System.IO;
using System.Text;
using System.Xml;

namespace System.Runtime.Serialization.Json
{
	public static class JsonReaderWriterFactory
	{
		public static XmlDictionaryReader CreateJsonReader (byte [] source, XmlDictionaryReaderQuotas quotas)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			return CreateJsonReader (source, 0, source.Length, quotas);
		}

		public static XmlDictionaryReader CreateJsonReader (byte [] source, int offset, int length, XmlDictionaryReaderQuotas quotas)
		{
			return CreateJsonReader (source, offset, length, Detect (source), quotas, null);
		}

		public static XmlDictionaryReader CreateJsonReader (byte [] source, int offset, int length, Encoding encoding, XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose readerClose)
		{
			return new JsonReader (source, offset, length, encoding, quotas, readerClose);
		}

		public static XmlDictionaryReader CreateJsonReader (Stream source, XmlDictionaryReaderQuotas quotas)
		{
			return CreateJsonReader (source, Detect (source), quotas, null);
		}

		public static XmlDictionaryReader CreateJsonReader (Stream source, Encoding encoding, XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose readerClose)
		{
			return new JsonReader (source, encoding, quotas, readerClose);
		}

		public static XmlDictionaryWriter CreateJsonWriter (Stream stream)
		{
			return CreateJsonWriter (stream, new UTF8Encoding (false, true));
		}

		public static XmlDictionaryWriter CreateJsonWriter (Stream stream, Encoding encoding)
		{
			return CreateJsonWriter (stream, encoding, false);
		}

		public static XmlDictionaryWriter CreateJsonWriter (Stream stream, Encoding encoding, bool closeOutput)
		{
			return new JsonWriter (stream, encoding, closeOutput);
		}

		static Encoding Detect (int b1, int b2)
		{
			if (b1 != -1 && b2 != -1) {
				if (b1 != 0 && b2 == 0)
					return new UnicodeEncoding (false, false, true);
				else if (b1 == 0 && b2 != 0)
					return new UnicodeEncoding (true, false, true);
			}
			return new UTF8Encoding (false, true);
		}

		static Encoding Detect (Stream source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			Stream stream = source;
			if (!stream.CanSeek)
				stream = new BufferedStream (source);
			Encoding e = Detect (stream.ReadByte(), stream.ReadByte());
			stream.Position = 0;
			return e;
		}

		static Encoding Detect (byte[] bytes)
		{
			if (bytes.Length < 2)
				return new UTF8Encoding (false, true);
			return Detect (bytes[0], bytes[1]);
		}
	}
}
