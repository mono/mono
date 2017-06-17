// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mono.Profiler.Log {

	public sealed class LogReader : IDisposable {

		public static Encoding Encoding { get; } = Encoding.UTF8;

		public LogStream BaseStream => (LogStream) _reader.BaseStream;

		public long Position { get; private set; }

		readonly BinaryReader _reader;

		public LogReader (LogStream stream, bool leaveOpen)
		{
			_reader = new BinaryReader (stream, Encoding, leaveOpen);
		}

		public void Dispose ()
		{
			_reader.Dispose ();
		}

		internal byte ReadByte ()
		{
			var b = _reader.ReadByte ();

			Position += sizeof (byte);

			return b;
		}

		internal ushort ReadUInt16 ()
		{
			var i = _reader.ReadUInt16 ();

			Position += sizeof (ushort);

			return i;
		}

		internal int ReadInt32 ()
		{
			var i = _reader.ReadInt32 ();

			Position += sizeof (int);

			return i;
		}

		internal long ReadInt64 ()
		{
			var i = _reader.ReadInt64 ();

			Position += sizeof (long);

			return i;
		}

		internal ulong ReadUInt64 ()
		{
			var i = _reader.ReadUInt64 ();

			Position += sizeof (ulong);

			return i;
		}

		internal double ReadDouble ()
		{
			var d = _reader.ReadDouble ();

			Position += sizeof (double);

			return d;
		}

		internal string ReadHeaderString ()
		{
			var bytes = _reader.ReadBytes (ReadInt32 ());

			Position += bytes.Length;

			return Encoding.GetString (bytes);
		}

		internal string ReadCString ()
		{
			var bytes = new List<byte> ();
			byte val;

			while ((val = ReadByte ()) != 0)
				bytes.Add (val);

			return Encoding.GetString (bytes.ToArray ());
		}

		internal long ReadSLeb128 ()
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

		internal ulong ReadULeb128 ()
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
