// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Text;

namespace Mono.Profiler.Log {

	sealed class LogReader : IDisposable {

		static readonly Encoding _encoding = Encoding.UTF8;

		readonly BinaryReader _reader;

		byte[] _stringBuffer = new byte [1024];

		public LogReader (Stream stream, bool leaveOpen)
		{
			_reader = new BinaryReader (stream, _encoding, leaveOpen);
		}

		public void Dispose ()
		{
			_reader.Dispose ();
		}

		public byte[] ReadBytes (int count)
		{
			var bytes = new byte [count];

			// BinaryReader.ReadBytes doesn't necessarily read the specified
			// amount of bytes, so just do it this way.
			for (var i = 0; i < bytes.Length; i++)
				bytes [i] = ReadByte ();

			return bytes;
		}

		public byte ReadByte ()
		{
			return _reader.ReadByte ();
		}

		public ushort ReadUInt16 ()
		{
			return _reader.ReadUInt16 ();
		}

		public int ReadInt32 ()
		{
			return _reader.ReadInt32 ();
		}

		public long ReadInt64 ()
		{
			return _reader.ReadInt64 ();
		}

		public ulong ReadUInt64 ()
		{
			return _reader.ReadUInt64 ();
		}

		public double ReadDouble ()
		{
			return _reader.ReadDouble ();
		}

		public string ReadHeaderString ()
		{
			return _encoding.GetString (ReadBytes (ReadInt32 ()));
		}

		public string ReadCString ()
		{
			var pos = 0;

			byte val;

			while ((val = ReadByte ()) != 0) {
				if (pos == _stringBuffer.Length)
					Array.Resize (ref _stringBuffer, System.Math.Max (_stringBuffer.Length * 2, pos + 1));

				_stringBuffer [pos++] = val;
			}

			return _encoding.GetString (_stringBuffer, 0, pos);
		}

		public long ReadSLeb128 ()
		{
			long result = 0;
			var shift = 0;

			while (true) {
				var b = ReadByte ();

				result |= (long) (b & 0x7f) << shift;
				shift += 7;

				if ((b & 0x80) != 0x80) {
					if (shift < sizeof (long) * 8 && (b & 0x40) == 0x40)
						result |= -(1L << shift);

					break;
				}
			}

			return result;
		}

		public ulong ReadULeb128 ()
		{
			ulong result = 0;
			var shift = 0;

			while (true) {
				var b = ReadByte ();

				result |= (ulong) (b & 0x7f) << shift;

				if ((b & 0x80) != 0x80)
					break;

				shift += 7;
			}

			return result;
		}
	}
}
