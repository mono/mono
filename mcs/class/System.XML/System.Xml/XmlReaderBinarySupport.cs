//
// System.Xml.XmlReaderBinarySupport.cs
//
// Author:
//   Atsushi Enomoto  (ginga@kit.hi-ho.ne.jp)
//
// (C)2004 Novell Inc,
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

using System;
using System.Collections;
using System.Text;

namespace System.Xml
{
	internal class XmlReaderBinarySupport
	{
		public delegate int CharGetter (
			char [] buffer, int offset, int length);

		public enum CommandState {
			None,
			ReadElementContentAsBase64,
			ReadContentAsBase64,
			ReadElementContentAsBinHex,
			ReadContentAsBinHex
		}

		public XmlReaderBinarySupport (XmlReader reader)
		{
			this.reader = reader;
			Reset ();
		}

		XmlReader reader;
		CharGetter getter;
		byte [] base64Cache = new byte [3];
		int base64CacheStartsAt;
		CommandState state;
		StringBuilder textCache;
		bool hasCache;
		bool dontReset;

		public CharGetter Getter {
			get { return getter; }
			set { getter = value; }
		}

		public void Reset ()
		{
			if (!dontReset) {
				dontReset = true;
				if (hasCache) {
					switch (reader.NodeType) {
					case XmlNodeType.Text:
					case XmlNodeType.CDATA:
					case XmlNodeType.SignificantWhitespace:
					case XmlNodeType.Whitespace:
						reader.Read ();
						break;
					}
					switch (state) {
					case CommandState.ReadElementContentAsBase64:
					case CommandState.ReadElementContentAsBinHex:
						reader.Read ();
						break;
					}
				}
				base64CacheStartsAt = -1;
				state = CommandState.None;
				hasCache = false;
				dontReset = false;
			}
		}

		InvalidOperationException StateError (CommandState action)
		{
			return new InvalidOperationException (
				String.Format ("Invalid attempt to read binary content by {0}, while once binary reading was started by {1}", action, state));
		}

		private void CheckState (bool element, CommandState action)
		{
			if (state == CommandState.None) {
				if (textCache == null)
					textCache = new StringBuilder ();
				else
					textCache.Length = 0;
				if (action == CommandState.None)
					return; // for ReadValueChunk()
				if (reader.ReadState != ReadState.Interactive)
					return;
				switch (reader.NodeType) {
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
				case XmlNodeType.SignificantWhitespace:
				case XmlNodeType.Whitespace:
					if (!element) {
						state = action;
						return;
					}
					break;
				case XmlNodeType.Element:
					if (element) {
						if (!reader.IsEmptyElement)
							reader.Read ();
						state = action;
						return;
					}
					break;
				}
				throw new XmlException ((element ? 
					"Reader is not positioned on an element."
					: "Reader is not positioned on a text node."));
			}
			if (state == action)
				return;
			throw StateError (action);
		}

		public int ReadElementContentAsBase64 (
			byte [] buffer, int offset, int length)
		{
			CheckState (true, CommandState.ReadElementContentAsBase64);
			return ReadBase64 (buffer, offset, length);
		}

		public int ReadContentAsBase64 (
			byte [] buffer, int offset, int length)
		{
			CheckState (false, CommandState.ReadContentAsBase64);
			return ReadBase64 (buffer, offset, length);
		}

		public int ReadElementContentAsBinHex (
			byte [] buffer, int offset, int length)
		{
			CheckState (true, CommandState.ReadElementContentAsBinHex);
			return ReadBinHex (buffer, offset, length);
		}

		public int ReadContentAsBinHex (
			byte [] buffer, int offset, int length)
		{
			CheckState (false, CommandState.ReadContentAsBinHex);
			return ReadBinHex (buffer, offset, length);
		}

		public int ReadBase64 (byte [] buffer, int offset, int length)
		{
			if (offset < 0)
				throw CreateArgumentOutOfRangeException ("offset", offset, "Offset must be non-negative integer.");
			else if (length < 0)
				throw CreateArgumentOutOfRangeException ("length", length, "Length must be non-negative integer.");
			else if (buffer.Length < offset + length)
				throw new ArgumentOutOfRangeException ("buffer length is smaller than the sum of offset and length.");

			if (reader.IsEmptyElement)
				return 0;
			if (length == 0)	// It does not raise an error.
				return 0;

			int bufIndex = offset;
			int bufLast = offset + length;

			if (base64CacheStartsAt >= 0) {
				for (int i = base64CacheStartsAt; i < 3; i++) {
					buffer [bufIndex++] = base64Cache [base64CacheStartsAt++];
					if (bufIndex == bufLast)
						return bufLast - offset;
				}
			}

			for (int i = 0; i < 3; i++)
				base64Cache [i] = 0;
			base64CacheStartsAt = -1;

			int max = (int) System.Math.Ceiling (4.0 / 3 * length);
			int additional = max % 4;
			if (additional > 0)
				max += 4 - additional;
			char [] chars = new char [max];
			int charsLength = getter != null ?
				getter (chars, 0, max) :
				ReadValueChunk (chars, 0, max);

			byte b = 0;
			byte work = 0;
			for (int i = 0; i < charsLength - 3; i++) {
				if ((i = SkipIgnorableBase64Chars (chars, charsLength, i)) == charsLength)
					break;
				b = (byte) (GetBase64Byte (chars [i]) << 2);
				if (bufIndex < bufLast)
					buffer [bufIndex] = b;
				else if (b != 0) {
					if (base64CacheStartsAt < 0)
						base64CacheStartsAt = 0;
					base64Cache [0] = b;
				}
				// charsLength mod 4 might not equals to 0.
				if (++i == charsLength)
					break;
				if ((i = SkipIgnorableBase64Chars (chars, charsLength, i))  == charsLength)
					break;
				b = GetBase64Byte (chars [i]);
				work = (byte) (b >> 4);
				if (bufIndex < bufLast) {
					buffer [bufIndex] += work;
					bufIndex++;
				}
				else if (work != 0) {
					if (base64CacheStartsAt < 0)
						base64CacheStartsAt = 0;
					base64Cache [0] += work;
				}

				work = (byte) ((b & 0xf) << 4);
				if (bufIndex < bufLast) {
					buffer [bufIndex] = work;
				}
				else if (work != 0) {
					if (base64CacheStartsAt < 0)
						base64CacheStartsAt = 1;
					base64Cache [1] = work;
				}

				if (++i == charsLength)
					break;
				if ((i = SkipIgnorableBase64Chars (chars, charsLength, i)) == charsLength)
					break;
				b = GetBase64Byte (chars [i]);
				work = (byte) (b >> 2);
				if (bufIndex < bufLast) {
					buffer [bufIndex] += work;
					bufIndex++;
				}
				else if (work != 0) {
					if (base64CacheStartsAt < 0)
						base64CacheStartsAt = 1;
					base64Cache [1] += work;
				}

				work = (byte) ((b & 3) << 6);
				if (bufIndex < bufLast)
					buffer [bufIndex] = work;
				else if (work != 0) {
					if (base64CacheStartsAt < 0)
						base64CacheStartsAt = 2;
					base64Cache [2] = work;
				}
				if (++i == charsLength)
					break;
				if ((i = SkipIgnorableBase64Chars (chars, charsLength, i)) == charsLength)
					break;
				work = GetBase64Byte (chars [i]);
				if (bufIndex < bufLast) {
					buffer [bufIndex] += work;
					bufIndex++;
				}
				else if (work != 0) {
					if (base64CacheStartsAt < 0)
						base64CacheStartsAt = 2;
					base64Cache [2] += work;
				}
			}
			int ret = System.Math.Min (bufLast - offset, bufIndex - offset);
			if (ret < length && charsLength > 0)
				return ret + ReadBase64 (buffer, offset + ret, length - ret);
			else
				return ret;
		}

		// Since ReadBase64() is processed for every 4 chars, it does
		// not handle '=' here.
		private byte GetBase64Byte (char ch)
		{
			switch (ch) {
			case '+':
				return 62;
			case '/':
				return 63;
			default:
				if (ch >= 'A' && ch <= 'Z')
					return (byte) (ch - 'A');
				else if (ch >= 'a' && ch <= 'z')
					return (byte) (ch - 'a' + 26);
				else if (ch >= '0' && ch <= '9')
					return (byte) (ch - '0' + 52);
				else
					throw new XmlException ("Invalid Base64 character was found.");
			}
		}

		private int SkipIgnorableBase64Chars (char [] chars, int charsLength, int i)
		{
			while (chars [i] == '=' || XmlChar.IsWhitespace (chars [i]))
				if (charsLength == ++i)
					break;
			return i;
		}

		static Exception CreateArgumentOutOfRangeException (string name, object value, string message)
		{
			return new ArgumentOutOfRangeException (
#if !NET_2_1
				name, value,
#endif
				message);
		}

		public int ReadBinHex (byte [] buffer, int offset, int length)
		{
			if (offset < 0)
				throw CreateArgumentOutOfRangeException ("offset", offset, "Offset must be non-negative integer.");
			else if (length < 0)
				throw CreateArgumentOutOfRangeException ("length", length, "Length must be non-negative integer.");
			else if (buffer.Length < offset + length)
				throw new ArgumentOutOfRangeException ("buffer length is smaller than the sum of offset and length.");

			if (length == 0)
				return 0;

			char [] chars = new char [length * 2];
			int charsLength = getter != null ?
				getter (chars, 0, length * 2) :
				ReadValueChunk (chars, 0, length * 2);
			return XmlConvert.FromBinHexString (chars, offset, charsLength, buffer);
		}

		public int ReadValueChunk (
			char [] buffer, int offset, int length)
		{
			CommandState backup = state;
			if (state == CommandState.None)
				CheckState (false, CommandState.None);

			if (offset < 0)
				throw CreateArgumentOutOfRangeException ("offset", offset, "Offset must be non-negative integer.");
			else if (length < 0)
				throw CreateArgumentOutOfRangeException ("length", length, "Length must be non-negative integer.");
			else if (buffer.Length < offset + length)
				throw new ArgumentOutOfRangeException ("buffer length is smaller than the sum of offset and length.");

			if (length == 0)
				return 0;

			if (!hasCache) {
				if (reader.IsEmptyElement)
					return 0;
			}

			bool loop = true;
			while (loop && textCache.Length < length) {
				switch (reader.NodeType) {
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
				case XmlNodeType.SignificantWhitespace:
				case XmlNodeType.Whitespace:
					if (hasCache) {
						switch (reader.NodeType) {
						case XmlNodeType.Text:
						case XmlNodeType.CDATA:
						case XmlNodeType.SignificantWhitespace:
						case XmlNodeType.Whitespace:
							Read ();
							break;
						default:
							loop = false;
							break;
						}
					}
					textCache.Append (reader.Value);
					hasCache = true;
					break;
				default:
					loop = false;
					break;
				}
			}
			state = backup;
			int min = textCache.Length;
			if (min > length)
				min = length;
			string str = textCache.ToString (0, min);
			textCache.Remove (0, str.Length);
			str.CopyTo (0, buffer, offset, str.Length);
			if (min < length && loop)
				return min + ReadValueChunk (buffer, offset + min, length - min);
			else
				return min;
		}

		private bool Read ()
		{
			dontReset = true;
			bool b = reader.Read ();
			dontReset = false;
			return b;
		}
	}
}
