using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace System.Runtime.Serialization.Json
{
	internal partial class JavaScriptObjectDeserializer
	{
		#region stream/reader classes

		public class BufferedStreamReader : StreamReader
		{
			public BufferedStreamReader (Stream stream)
				: base (new BufferedStream (stream))
			{
			}
			
			BufferedStreamReader (BufferedStream stream)
				: base (stream, JavaScriptObjectDeserializer.DetectEncoding (stream.first, stream.second))
			{
			}
		}

		class BufferedStream : Stream
		{
			internal int first, second;
			long pos;
			Stream source;

			public BufferedStream (Stream source)
			{
				this.source = source;
				first = source.ReadByte ();
				second = source.ReadByte ();
			}

			public override int Read (byte [] buffer, int index, int count)
			{
				if (buffer == null)
					throw new ArgumentNullException ("buffer");
				if (index < 0 || index >= buffer.Length)
					throw new ArgumentOutOfRangeException ("index");
				if (count < 0 || index + count > buffer.Length)
					throw new ArgumentOutOfRangeException ("count");

				if (count == 0)
					return 0;
				int iniCount = count;
				if (pos == 0) {
					buffer [index++] = (byte) first;
					pos++;
					if (--count == 0)
						return iniCount;
				}
				if (pos == 1) {
					buffer [index++] = (byte) second;
					pos++;
					if (--count == 0)
						return iniCount;
				}
				return source.Read (buffer, index, count);
			}

			public override int ReadByte ()
			{
				switch (pos) {
				case 0:
					pos++;
					return first;
				case 1:
					pos++;
					return second;
				default:
					return source.ReadByte ();
				}
			}

			public override bool CanRead {
				get { return source.CanRead; }
			}

			public override bool CanSeek {
				get { return false; }
			}

			public override bool CanWrite {
				get { return false; }
			}

			public override long Length {
				get { return source.Length; }
			}

			public override long Position {
				get { return source.Position; }
				set {
					if (value < 2)
						pos = value;
					source.Position = value;
				}
			}

			public override long Seek (long pos, SeekOrigin origin)
			{
				throw new NotSupportedException ();
			}

			public override void SetLength (long pos)
			{
				throw new NotSupportedException ();
			}

			public override void Write (byte [] buf, int index, int count)
			{
				throw new NotSupportedException ();
			}

			public override void Flush ()
			{
				// do nothing
			}
		}

		#endregion

		public JavaScriptObjectDeserializer (string json, bool raiseNumberParseError)
		{
			reader = new JavaScriptReader (new StringReader (json), raiseNumberParseError);
		}

		JavaScriptReader reader;

		public object BasicDeserialize ()
		{
			return reader.Read ();
		}

		public static Encoding DetectEncoding (int byte1, int byte2)
		{
			switch (byte1) {
			case 0:
				if (byte2 == 0)
					throw new XmlException ("UTF-32BE is detected, which is not supported");
				else
					return Encoding.UTF8;
			case 0xFE:
				if (byte2 == 0xFF)
					return Encoding.BigEndianUnicode;
				else
					return Encoding.UTF8;
			case 0xFF:
				if (byte2 == 0xFE) // could be UTF-32LE, but there is no way to detect that only within two bytes.
					return Encoding.Unicode;
				else
					return Encoding.UTF8;
			default:
				return Encoding.UTF8;
			}
		}
	}
}
