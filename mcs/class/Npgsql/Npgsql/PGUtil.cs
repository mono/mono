// created on 1/6/2002 at 22:27

// Npgsql.PGUtil.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Npgsql
{
	///<summary>
	/// This class provides many util methods to handle
	/// reading and writing of PostgreSQL protocol messages.
	/// </summary>
	internal static class PGUtil
	{
		// Logging related values
		private static readonly String CLASSNAME = "PGUtil";
		internal static readonly ResourceManager resman = new ResourceManager(MethodBase.GetCurrentMethod().DeclaringType);
		//TODO: What should this value be?
		//There is an obvious balancing act in setting this value. The larger the value, the fewer times
		//we need to use it and the more efficient we are in that regard. The smaller the value, the
		//less space is used, and the more efficient we are in that regard.
		//Multiples of memory page size are often good, which tends to be 4096 (4kB) and hence 4096 is
		//often used for cases like this (e.g. the default size of the buffer in System.IO.BufferedStream).
		//This is hard to guess, and even harder to guess if any overhead will be involved making a value
		//of 4096 * x - overhead the real ideal with this approach.
		//If this buffer were per-instance in a non-static class then 4096 would probably be the one to go for,
		//but since this buffer is shared we can perhaps afford to go a bit larger.
		//
		//Another potentially useful value, since we will be using a network stream which in turn is using
		//a TCP/IP stream, is the size of the TCP/IP window. This is even harder to predict than
		//memory page size, but is likely to be 1460 * 44 = 64240. It could be lower or higher but experimentation
		//shows that on at least the experimentation machine (.NET on WindowsXP, connecting to postgres server on
		//localhost) if more than 64240 was available only 64240 would be used in each pass), so for at least
		//that machine anything higher than 64240 is a waste.
		//64240 being a bit much, settling for 4096 for now.
		private const int THRASH_CAN_SIZE = 4096;
		//This buffer array is used for reading and ignoring bytes we want to discard.
		//It is thread-safe solely because it is never read from!
		//Any attempt to read from this array deserves to fail.
		//Note that in the cases where we are reading bytes to actually process them, we either
		//have a buffer supplied from elsewhere (the calling method) or we create a small buffer
		//as needed. This is since the buffer being used means that there that the possible performance
		//gain of not creating a new buffer will be cancelled out by whatever buffering will have to be
		//done to actually use the data. This is not the case here - we are pre-assigning a buffer for
		//this case purely because we don't care what gets put into it.
		private static readonly byte[] THRASH_CAN = new byte[THRASH_CAN_SIZE];
		private static readonly Encoding ENCODING_UTF8 = Encoding.UTF8;

		///<summary>
		/// This method takes a ProtocolVersion and returns an integer
		/// version number that the Postgres backend will recognize in a
		/// startup packet.
		/// </summary>
		public static Int32 ConvertProtocolVersion(ProtocolVersion Ver)
		{
			switch (Ver)
			{
				case ProtocolVersion.Version2:
					return (int) ServerVersionCode.ProtocolVersion2;

				case ProtocolVersion.Version3:
					return (int) ServerVersionCode.ProtocolVersion3;
			}

			throw new ArgumentOutOfRangeException();
		}

		/// <summary>
		/// This method takes a version string as returned by SELECT VERSION() and returns
		/// a valid version string ("7.2.2" for example).
		/// This is only needed when running protocol version 2.
		/// This does not do any validity checks.
		/// </summary>
		public static string ExtractServerVersion(string VersionString)
		{
			Int32 Start = 0, End = 0;

			// find the first digit and assume this is the start of the version number
			for (; Start < VersionString.Length && !char.IsDigit(VersionString[Start]); Start++)
			{
				;
			}

			End = Start;

			// read until hitting whitespace, which should terminate the version number
			for (; End < VersionString.Length && !char.IsWhiteSpace(VersionString[End]); End++)
			{
				;
			}

			// Deal with this here so that if there are 
			// changes in a future backend version, we can handle it here in the
			// protocol handler and leave everybody else put of it.

			VersionString = VersionString.Substring(Start, End - Start + 1);

			for (int idx = 0; idx != VersionString.Length; ++idx)
			{
				char c = VersionString[idx];
				if (!char.IsDigit(c) && c != '.')
				{
					VersionString = VersionString.Substring(0, idx);
					break;
				}
			}

			return VersionString;
		}

/*
		/// <summary>
		/// Convert the beginning numeric part of the given string to Int32.
		/// For example:
		///   Strings "12345ABCD" and "12345.54321" would both be converted to int 12345.
		/// </summary>
		private static Int32 ConvertBeginToInt32(String Raw)
		{
			Int32 Length = 0;
			for (; Length < Raw.Length && Char.IsNumber(Raw[Length]); Length++)
			{
				;
			}
			return Convert.ToInt32(Raw.Substring(0, Length));
		}
*/

		///<summary>
		/// This method gets a C NULL terminated string from the network stream.
		/// It keeps reading a byte in each time until a NULL byte is returned.
		/// It returns the resultant string of bytes read.
		/// This string is sent from backend.
		/// </summary>
		public static String ReadString(Stream network_stream)
		{
			NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ReadString");

			List<byte> buffer = new List<byte>();
			for (int bRead = network_stream.ReadByte(); bRead != 0; bRead = network_stream.ReadByte())
			{
				if (bRead == -1)
				{
					throw new IOException();
				}
				else
				{
					buffer.Add((byte) bRead);
				}
			}

            if (NpgsqlEventLog.Level >= LogLevel.Debug)
                NpgsqlEventLog.LogMsg(resman, "Log_StringRead", LogLevel.Debug, ENCODING_UTF8.GetString(buffer.ToArray()));
                
			return ENCODING_UTF8.GetString(buffer.ToArray());
		}

		public static char ReadChar(Stream stream)
		{
			byte[] buffer = new byte[4]; //No character is more than 4 bytes long.
			for (int i = 0; i != 4; ++i)
			{
				int byteRead = stream.ReadByte();
				if (byteRead == -1)
				{
					throw new EndOfStreamException();
				}
				buffer[i] = (byte) byteRead;
				if (ValidUTF8Ending(buffer, 0, i + 1)) //catch multi-byte encodings where we have not yet enough bytes.
				{
					return ENCODING_UTF8.GetChars(buffer)[0];
				}
			}
			throw new InvalidDataException();
		}

		public static int ReadChars(Stream stream, char[] output, int maxChars, ref int maxRead, int outputIdx)
		{
			if (maxChars == 0 || maxRead == 0)
			{
				return 0;
			}
			byte[] buffer = new byte[Math.Min(maxRead, maxChars*4)];
			int bytesSoFar = 0;
			int charsSoFar = 0;
			//A string of x chars length will take at least x bytes and at most
			//4x. We set our buffer to 4x in size, but start with only reading x bytes.
			//If we don't have the full string at this point, then we now have a new number
			//of characters to read, and so we try to read one byte per character remaining to read.
			//This hence involves the fewest possible number of passes through CheckedStreamRead
			//without the risk of accidentally reading too far into the stream.
			do
			{
				int toRead = Math.Min(maxRead, maxChars - charsSoFar);
				CheckedStreamRead(stream, buffer, bytesSoFar, toRead);
				maxRead -= toRead;
				bytesSoFar += toRead;
			}
			while (maxRead > 0 && (charsSoFar = PessimisticGetCharCount(buffer, 0, bytesSoFar)) < maxChars);
			return ENCODING_UTF8.GetDecoder().GetChars(buffer, 0, bytesSoFar, output, outputIdx, false);
		}

		public static int SkipChars(Stream stream, int maxChars, ref int maxRead)
		{
			//This is the same as ReadChars, but it just discards the characters read.
			if (maxChars == 0 || maxRead == 0)
			{
				return 0;
			}
			byte[] buffer = new byte[Math.Min(maxRead, ENCODING_UTF8.GetMaxByteCount(maxChars))];
			int bytesSoFar = 0;
			int charsSoFar = 0;
			do
			{
				int toRead = Math.Min(maxRead, maxChars - charsSoFar);
				CheckedStreamRead(stream, buffer, bytesSoFar, toRead);
				maxRead -= toRead;
				bytesSoFar += toRead;
			}
			while (maxRead > 0 && (charsSoFar = PessimisticGetCharCount(buffer, 0, bytesSoFar)) < maxChars);
			return charsSoFar;
		}

		// Encoding.GetCharCount will count an incomplete character as a character.
		// This makes sense if the user wants to assign a char[] buffer, since the incomplete character
		// might be represented as U+FFFD or a similar approach may be taken.
		// It's not much use though if we know the stream has >= x characters in it and we
		// want to read x complete characters - we need to know that the last character is complete.
		// Therefore we need to check on this.
		// SECURITY CONSIDERATIONS:
		// This function assumes we recieve valid UTF-8 data and as such security considerations
		// must be noticed.
		// In the case of deliberate mal-formed UTF-8 data, this function will not be used
		// in it's interpretation. In the worse case it will result in the stream being mis-read
		// and the operation malfunctioning, but anyone who is acting as a man-in-the-middle
		// on the stream can already do that to us in a variety of interesting ways, so this
		// function does not introduce a security issue in this regard.
		// Therefore the optimisation of assuming valid UTF-8 data is reasonable and not insecure.
		public static bool ValidUTF8Ending(byte[] buffer, int index, int count)
		{
			if (count <= 0)
			{
				return false;
			}
			byte examine = buffer[index + count - 1];
			//simplest case and also the most common with most data- a single-octet character. Handle directly.
			if ((examine & 0x80) == 0)
			{
				return true;
			}
			//if the last byte isn't a trailing byte we're clearly not at the end.
			if ((examine & 0xC0) != 0x80)
			{
				return false;
			}
			byte[] masks = new byte[] {0xE0, 0xF0, 0xF8};
			byte[] matches = new byte[] {0xC0, 0xE0, 0xF0};
			for (int i = 0; i + 2 <= count; ++i)
			{
				examine = buffer[index + count - 2 - i];
				if ((examine & masks[i]) == matches[i])
				{
					return true;
				}
				if ((examine & 0xC0) != 0x80)
				{
					return false;
				}
			}
			return false;
		}

		//This is like Encoding.UTF8.GetCharCount() but it ignores a trailing incomplete
		//character. See comments on ValidUTF8Ending()
		public static int PessimisticGetCharCount(byte[] buffer, int index, int count)
		{
			return ENCODING_UTF8.GetCharCount(buffer, index, count) - (ValidUTF8Ending(buffer, index, count) ? 0 : 1);
		}

		///<summary>
		/// This method writes a C NULL terminated string to the network stream.
		/// It appends a NULL terminator to the end of the String.
		/// </summary>
		///<summary>
		/// This method writes a C NULL terminated string to the network stream.
		/// It appends a NULL terminator to the end of the String.
		/// </summary>
		public static void WriteString(String the_string, Stream network_stream)
		{
			NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "WriteString");

			NpgsqlEventLog.LogMsg(resman, "Log_StringWritten", LogLevel.Debug, the_string);

			network_stream.Write(ENCODING_UTF8.GetBytes(the_string + '\x00'), 0, ENCODING_UTF8.GetByteCount(the_string) + 1);
		}

		///<summary>
		/// This method writes a C NULL terminated string limited in length to the
		/// backend server.
		/// It pads the string with null bytes to the size specified.
		/// </summary>
		public static void WriteLimString(String the_string, Int32 n, Stream network_stream)
		{
			NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "WriteLimString");

			//Note: We do not know the size in bytes until after we have converted the string.
			byte[] bytes = ENCODING_UTF8.GetBytes(the_string);
			if (bytes.Length > n)
			{
				throw new ArgumentOutOfRangeException("the_string", the_string,
				                                      string.Format(resman.GetString("LimStringWriteTooLarge"), the_string, n));
			}

			network_stream.Write(bytes, 0, bytes.Length);

			//pad with zeros.
			if (bytes.Length < n)
			{
				bytes = new byte[n - bytes.Length];
				network_stream.Write(bytes, 0, bytes.Length);
			}
		}

		public static void CheckedStreamRead(Stream stream, Byte[] buffer, Int32 offset, Int32 size)
		{
			Int32 bytes_from_stream = 0;
			Int32 total_bytes_read = 0;

			while (size > 0)
			{
				bytes_from_stream = stream.Read(buffer, offset + total_bytes_read, size);
				total_bytes_read += bytes_from_stream;
				size -= bytes_from_stream;
			}
		}

		public static void EatStreamBytes(Stream stream, int size)
		{
//See comment on THRASH_CAN and THRASH_CAN_SIZE.
			while (size > 0)
			{
				size -= stream.Read(THRASH_CAN, 0, size < THRASH_CAN_SIZE ? size : THRASH_CAN_SIZE);
			}
		}

		public static int ReadEscapedBytes(Stream stream, byte[] output, int maxBytes, ref int maxRead, int outputIdx)
		{
			maxBytes = maxBytes > output.Length - outputIdx ? output.Length - outputIdx : maxBytes;
			int i;
			for (i = 0; i != maxBytes && maxRead > 0; ++i)
			{
				char c = ReadChar(stream);
				--maxRead;
				if (c == '\\')
				{
					--maxRead;
					switch (c = ReadChar(stream))
					{
						case '0':
						case '1':
						case '2':
						case '3':
						case '4':
						case '5':
						case '6':
						case '7':
						case '8':
						case '9':
							maxRead -= 2;
							output[outputIdx++] =
								(byte)
								((int.Parse(c.ToString()) << 6) | (int.Parse(ReadChar(stream).ToString()) << 3) |
								 int.Parse(ReadChar(stream).ToString()));
							break;
						default:
							output[outputIdx++] = (byte) c;
							break;
					}
				}
				else
				{
					output[outputIdx++] = (byte) c;
				}
			}
			return i;
		}

		public static int SkipEscapedBytes(Stream stream, int maxBytes, ref int maxRead)
		{
			int i;
			for (i = 0; i != maxBytes && maxRead > 0; ++i)
			{
				--maxRead;
				if (ReadChar(stream) == '\\')
				{
					--maxRead;
					switch (ReadChar(stream))
					{
						case '0':
						case '1':
						case '2':
						case '3':
						case '4':
						case '5':
						case '6':
						case '7':
						case '8':
						case '9':
							maxRead -= 2;
							EatStreamBytes(stream, 2); //note assumes all representations of '0' through '9' are single-byte.
							break;
					}
				}
			}
			return i;
		}

		/// <summary>
		/// Write a 32-bit integer to the given stream in the correct byte order.
		/// </summary>
		public static void WriteInt32(Stream stream, Int32 value)
		{
			stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value)), 0, 4);
		}

		/// <summary>
		/// Read a 32-bit integer from the given stream in the correct byte order.
		/// </summary>
		public static Int32 ReadInt32(Stream stream)
		{
			byte[] buffer = new byte[4];
			CheckedStreamRead(stream, buffer, 0, 4);
			return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, 0));
		}

		/// <summary>
		/// Write a 16-bit integer to the given stream in the correct byte order.
		/// </summary>
		public static void WriteInt16(Stream stream, Int16 value)
		{
			stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value)), 0, 2);
		}

		/// <summary>
		/// Read a 16-bit integer from the given stream in the correct byte order.
		/// </summary>
		public static Int16 ReadInt16(Stream stream)
		{
			byte[] buffer = new byte[2];
			CheckedStreamRead(stream, buffer, 0, 2);
			return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 0));
		}

		public static int RotateShift(int val, int shift)
		{
			return (val << shift) | (val >> (sizeof (int) - shift));
		}
	}

	/// <summary>
	/// Represent the frontend/backend protocol version.
	/// </summary>
	public enum ProtocolVersion
	{
		Unknown,
		Version2,
		Version3
	}

	public enum ServerVersionCode
	{
		ProtocolVersion2 = 2 << 16, // 131072
		ProtocolVersion3 = 3 << 16 // 196608
	}

	/// <summary>
	/// Represent the backend server version.
	/// As this class offers no functionality beyond that offered by <see cref="System.Version"/> it has been
	/// deprecated in favour of that class.
	/// </summary>
	/// 
	[Obsolete("Use System.Version")]
	public sealed class ServerVersion : IEquatable<ServerVersion>, IComparable<ServerVersion>, IComparable, ICloneable
	{
		[Obsolete("Use ServerVersionCode.ProtocolVersion2")] public static readonly Int32 ProtocolVersion2 = 2 << 16;
		                                                                                  // 131072

		[Obsolete("Use ServerVersionCode.ProtocolVersion3")] public static readonly Int32 ProtocolVersion3 = 3 << 16;
		                                                                                  // 196608

		private readonly Version _version;

		private ServerVersion(Version ver)
		{
			_version = ver;
		}

		/// <summary>
		/// Server version major number.
		/// </summary>
		public Int32 Major
		{
			get { return _version.Major; }
		}

		/// <summary>
		/// Server version minor number.
		/// </summary>
		public Int32 Minor
		{
			get { return _version.Minor; }
		}

		/// <summary>
		/// Server version patch level number.
		/// </summary>
		public Int32 Patch
		{
			get { return _version.Build; }
		}

		public static implicit operator Version(ServerVersion sv)
		{
			return (object) sv == null ? null : sv._version;
		}

		public static implicit operator ServerVersion(Version ver)
		{
			return (object) ver == null ? null : new ServerVersion(ver.Clone() as Version);
		}

		public static bool operator ==(ServerVersion One, ServerVersion TheOther)
		{
			return ((Version) One) == ((Version) TheOther);
		}

		public static bool operator !=(ServerVersion One, ServerVersion TheOther)
		{
			return (Version) One != (Version) TheOther;
		}

		public static bool operator >(ServerVersion One, ServerVersion TheOther)
		{
			return (Version) One > (Version) TheOther;
		}

		public static bool operator >=(ServerVersion One, ServerVersion TheOther)
		{
			return (Version) One >= (Version) TheOther;
		}

		public static bool operator <(ServerVersion One, ServerVersion TheOther)
		{
			return (Version) One < (Version) TheOther;
		}

		public static bool operator <=(ServerVersion One, ServerVersion TheOther)
		{
			return (Version) One <= (Version) TheOther;
		}

		public bool Equals(ServerVersion other)
		{
			return other != null && this == other;
		}

		public int CompareTo(ServerVersion other)
		{
			return _version.CompareTo(other);
		}

		public int CompareTo(object obj)
		{
			return CompareTo(obj as ServerVersion);
		}

		public object Clone()
		{
			return new ServerVersion(_version.Clone() as Version);
		}

		public override bool Equals(object O)
		{
			return Equals(O as ServerVersion);
		}

		public override int GetHashCode()
		{
			//Assume Version has a decent hash code function.
			//If this turns out not to be true do not use _Major ^ _Minor ^ _Patch, but make sure the values are spread throughout the 32bit range more to avoid false clashes.
			return _version.GetHashCode();
		}

		/// <summary>
		/// Returns the string representation of this version in three place dot notation (Major.Minor.Patch).
		/// </summary>
		public override String ToString()
		{
			return _version.ToString();
		}
	}

	internal enum FormatCode :
		short
	{
		Text = 0,
		Binary = 1
	}

	internal class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		private readonly Dictionary<TKey, TValue> _inner;

		public ReadOnlyDictionary(Dictionary<TKey, TValue> inner)
		{
			_inner = inner;
		}

		private static void StopWrite()
		{
			throw new NotSupportedException(PGUtil.resman.GetString("Read_Only_Write_Error"));
		}

		public TValue this[TKey key]
		{
			get { return _inner[key]; }
			set { StopWrite(); }
		}

		public ICollection<TKey> Keys
		{
			get { return _inner.Keys; }
		}

		public ICollection<TValue> Values
		{
			get { return _inner.Values; }
		}

		public int Count
		{
			get { return _inner.Count; }
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		public bool ContainsKey(TKey key)
		{
			return _inner.ContainsKey(key);
		}

		public void Add(TKey key, TValue value)
		{
			StopWrite();
		}

		public bool Remove(TKey key)
		{
			StopWrite();
			return false;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return _inner.TryGetValue(key, out value);
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			StopWrite();
		}

		public void Clear()
		{
			StopWrite();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return ((IDictionary<TKey, TValue>) _inner).Contains(item);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			((IDictionary<TKey, TValue>) _inner).CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			StopWrite();
			return false;
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			foreach (KeyValuePair<TKey, TValue> kvp in _inner)
			{
				yield return new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value); //return copy so changes don't affect our copy.
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _inner.GetEnumerator();
		}
	}
}