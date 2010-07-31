/*
 * ByteEncoding.cs - Implementation of the "I18N.Common.ByteEncoding" class.
 *
 * Copyright (c) 2002  Southern Storm Software, Pty Ltd
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR
 * OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */

namespace I18N.Common
{

using System;
using System.Runtime.InteropServices;
using System.Text;

// This class provides an abstract base for encodings that use a single
// byte per character.  The bulk of the work is done in this class, with
// subclasses providing implementations of the "ToBytes" methods to perform
// the char->byte conversion.

[Serializable]
public abstract class ByteEncoding : MonoEncoding
{
	// Internal state.
	protected char[] toChars;
	protected String encodingName;
	protected String bodyName;
	protected String headerName;
	protected String webName;
	protected bool isBrowserDisplay;
	protected bool isBrowserSave;
	protected bool isMailNewsDisplay;
	protected bool isMailNewsSave;
	protected int windowsCodePage;
#if NET_2_0
	static byte [] isNormalized;
	static byte [] isNormalizedComputed;
	static byte [] normalization_bytes;
#endif

	// Constructor.
	protected ByteEncoding(int codePage, char[] toChars,
						   String encodingName, String bodyName,
						   String headerName, String webName,
						   bool isBrowserDisplay, bool isBrowserSave,
						   bool isMailNewsDisplay, bool isMailNewsSave,
						   int windowsCodePage)
			: base(codePage)
			{
				if (toChars.Length != byte.MaxValue + 1)
					throw new ArgumentException("toChars");

				this.toChars = toChars;
				this.encodingName = encodingName;
				this.bodyName = bodyName;
				this.headerName = headerName;
				this.webName = webName;
				this.isBrowserDisplay = isBrowserDisplay;
				this.isBrowserSave = isBrowserSave;
				this.isMailNewsDisplay = isMailNewsDisplay;
				this.isMailNewsSave = isMailNewsSave;
				this.windowsCodePage = windowsCodePage;
			}

#if NET_2_0
	public override bool IsAlwaysNormalized (NormalizationForm form)
	{
		if (form != NormalizationForm.FormC)
			return false;

		if (isNormalized == null)
			isNormalized = new byte [0x10000 / 8];
		if (isNormalizedComputed == null)
			isNormalizedComputed = new byte [0x10000 / 8];

		if (normalization_bytes == null) {
			normalization_bytes = new byte [0x100];
			lock (normalization_bytes) {
				for (int i = 0; i < 0x100; i++)
					normalization_bytes [i] = (byte) i;
			}
		}

		byte offset = (byte) (1 << (CodePage % 8));
		if ((isNormalizedComputed [CodePage / 8] & offset) == 0) {
			Encoding e = Clone () as Encoding;
			e.DecoderFallback = new DecoderReplacementFallback ("");
			string s = e.GetString (normalization_bytes);
			// note that the flag only stores FormC information.
			if (s != s.Normalize (form))
				isNormalized [CodePage / 8] |= offset;
			isNormalizedComputed [CodePage / 8] |= offset;
		}

		return (isNormalized [CodePage / 8] & offset) == 0;
	}

	public override bool IsSingleByte {
		get { return true; }
	}
#endif

	public override int GetByteCount(String s)
			{
				if(s == null)
				{
					throw new ArgumentNullException("s");
				}
				return s.Length;
			}

	// Get the number of bytes needed to encode a character buffer.
	public unsafe override int GetByteCountImpl (char* chars, int count)
			{
				return count;
			}

	// Convert an array of characters into a byte buffer,
	// once the parameters have been validated.
	protected unsafe abstract void ToBytes (
		char* chars, int charCount, byte* bytes, int byteCount);
	/*
	protected unsafe virtual void ToBytes (
		char* chars, int charCount, byte* bytes, int byteCount)
	{
		// When it is not overriden, use ToBytes() with arrays.
		char [] carr = new char [charCount];
		Marshal.Copy ((IntPtr) chars, carr, 0, charCount);
		byte [] barr = new byte [byteCount];
		Marshal.Copy ((IntPtr) bytes, barr, 0, byteCount);
		ToBytes (carr, 0, charCount, barr, 0);
	}
	*/

	// Convert an array of characters into a byte buffer,
	// once the parameters have been validated.
	protected unsafe virtual void ToBytes(char[] chars, int charIndex, int charCount,
									byte[] bytes, int byteIndex)
	{
		// When it is not overriden, use ToBytes() with pointers
		// (this is the ideal solution)
		if (charCount == 0 || bytes.Length == byteIndex)
			return;
		fixed (char* cptr = chars) {
			fixed (byte* bptr = bytes) {
				ToBytes (cptr + charIndex, charCount,
					bptr + byteIndex, bytes.Length - byteIndex);
			}
		}
	}

/*
	// Convert a string into a byte buffer, once the parameters
	// have been validated.
	protected unsafe virtual void ToBytes(String s, int charIndex, int charCount,
									byte[] bytes, int byteIndex)
	{
		// When it is not overriden, use ToBytes() with pointers
		// (Ideal solution)
		if (s.Length == 0 || bytes.Length == byteIndex)
			return;
		fixed (char* cptr = s) {
			fixed (byte* bptr = bytes) {
				ToBytes (cptr + charIndex, charCount,
					bptr + byteIndex, bytes.Length - byteIndex);
			}
		}
	}
*/

	//[CLSCompliant (false)]
	public unsafe override int GetBytesImpl (char* chars, int charCount, byte* bytes, int byteCount)
	{
		ToBytes (chars, charCount, bytes, byteCount);
		return charCount;
	}
/*
	// Get the bytes that result from encoding a character buffer.
	public override int GetBytes(char[] chars, int charIndex, int charCount,
								 byte[] bytes, int byteIndex)
			{
				if(chars == null)
				{
					throw new ArgumentNullException("chars");
				}
				if(bytes == null)
				{
					throw new ArgumentNullException("bytes");
				}
				if(charIndex < 0 || charIndex > chars.Length)
				{
					throw new ArgumentOutOfRangeException
						("charIndex", Strings.GetString("ArgRange_Array"));
				}
				if(charCount < 0 || charCount > (chars.Length - charIndex))
				{
					throw new ArgumentOutOfRangeException
						("charCount", Strings.GetString("ArgRange_Array"));
				}
				if(byteIndex < 0 || byteIndex > bytes.Length)
				{
					throw new ArgumentOutOfRangeException
						("byteIndex", Strings.GetString("ArgRange_Array"));
				}
				if((bytes.Length - byteIndex) < charCount)
				{
					throw new ArgumentException
						(Strings.GetString("Arg_InsufficientSpace"));
				}
				ToBytes(chars, charIndex, charCount, bytes, byteIndex);
				return charCount;
			}

	// Convenience wrappers for "GetBytes".
	public override int GetBytes(String s, int charIndex, int charCount,
								 byte[] bytes, int byteIndex)
			{
				if(s == null)
				{
					throw new ArgumentNullException("s");
				}
				if(bytes == null)
				{
					throw new ArgumentNullException("bytes");
				}
				if(charIndex < 0 || charIndex > s.Length)
				{
					throw new ArgumentOutOfRangeException
						("charIndex",
						 Strings.GetString("ArgRange_StringIndex"));
				}
				if(charCount < 0 || charCount > (s.Length - charIndex))
				{
					throw new ArgumentOutOfRangeException
						("charCount",
						 Strings.GetString("ArgRange_StringRange"));
				}
				if(byteIndex < 0 || byteIndex > bytes.Length)
				{
					throw new ArgumentOutOfRangeException
						("byteIndex", Strings.GetString("ArgRange_Array"));
				}
				if((bytes.Length - byteIndex) < charCount)
				{
					throw new ArgumentException
						(Strings.GetString("Arg_InsufficientSpace"));
				}
				ToBytes(s, charIndex, charCount, bytes, byteIndex);
				return charCount;
			}
*/

	// Get the number of characters needed to decode a byte buffer.
	public override int GetCharCount(byte[] bytes, int index, int count)
			{
				if(bytes == null)
				{
					throw new ArgumentNullException("bytes");
				}
				if(index < 0 || index > bytes.Length)
				{
					throw new ArgumentOutOfRangeException
						("index", Strings.GetString("ArgRange_Array"));
				}
				if(count < 0 || count > (bytes.Length - index))
				{
					throw new ArgumentOutOfRangeException
						("count", Strings.GetString("ArgRange_Array"));
				}
				return count;
			}

	// Get the characters that result from decoding a byte buffer.
	public override int GetChars(byte[] bytes, int byteIndex, int byteCount,
								 char[] chars, int charIndex)
			{
				if(bytes == null)
				{
					throw new ArgumentNullException("bytes");
				}
				if(chars == null)
				{
					throw new ArgumentNullException("chars");
				}
				if(byteIndex < 0 || byteIndex > bytes.Length)
				{
					throw new ArgumentOutOfRangeException
						("byteIndex", Strings.GetString("ArgRange_Array"));
				}
				if(byteCount < 0 || byteCount > (bytes.Length - byteIndex))
				{
					throw new ArgumentOutOfRangeException
						("byteCount", Strings.GetString("ArgRange_Array"));
				}
				if(charIndex < 0 || charIndex > chars.Length)
				{
					throw new ArgumentOutOfRangeException
						("charIndex", Strings.GetString("ArgRange_Array"));
				}
				if((chars.Length - charIndex) < byteCount)
				{
					throw new ArgumentException
						(Strings.GetString("Arg_InsufficientSpace"));
				}
				int count = byteCount;
				char[] cvt = toChars;
				while(count-- > 0)
				{
					chars[charIndex++] = cvt[(int)(bytes[byteIndex++])];
				}
				return byteCount;
			}

	// Get the maximum number of bytes needed to encode a
	// specified number of characters.
	public override int GetMaxByteCount(int charCount)
			{
				if(charCount < 0)
				{
					throw new ArgumentOutOfRangeException
						("charCount",
						 Strings.GetString("ArgRange_NonNegative"));
				}
				return charCount;
			}

	// Get the maximum number of characters needed to decode a
	// specified number of bytes.
	public override int GetMaxCharCount(int byteCount)
			{
				if(byteCount < 0)
				{
					throw new ArgumentOutOfRangeException
						("byteCount",
						 Strings.GetString("ArgRange_NonNegative"));
				}
				return byteCount;
			}

	// Decode a buffer of bytes into a string.
	public unsafe override String GetString(byte[] bytes, int index, int count)
			{
				if(bytes == null)
				{
					throw new ArgumentNullException("bytes");
				}
				if(index < 0 || index > bytes.Length)
				{
					throw new ArgumentOutOfRangeException
						("index", Strings.GetString("ArgRange_Array"));
				}
				if(count < 0 || count > (bytes.Length - index))
				{
					throw new ArgumentOutOfRangeException
						("count", Strings.GetString("ArgRange_Array"));
				}

				if (count == 0)
					return string.Empty;

				string s = new string ((char) 0, count);

				fixed (byte* bytePtr = bytes)
					fixed (char* charPtr = s)
						fixed (char* cvt = toChars) {
							byte* b = bytePtr + index;
							char* c = charPtr;
							while(count-- != 0)
								*(c++) = cvt[*(b++)];
						}

				return s;
			}
	public override String GetString(byte[] bytes)
			{
				if(bytes == null)
				{
					throw new ArgumentNullException("bytes");
				}

				return GetString (bytes, 0, bytes.Length);
			}

#if !ECMA_COMPAT

	// Get the mail body name for this encoding.
	public override String BodyName
			{
				get
				{
					return bodyName;
				}
			}

	// Get the human-readable name for this encoding.
	public override String EncodingName
			{
				get
				{
					return encodingName;
				}
			}

	// Get the mail agent header name for this encoding.
	public override String HeaderName
			{
				get
				{
					return headerName;
				}
			}

	// Determine if this encoding can be displayed in a Web browser.
	public override bool IsBrowserDisplay
			{
				get
				{
					return isBrowserDisplay;
				}
			}

	// Determine if this encoding can be saved from a Web browser.
	public override bool IsBrowserSave
			{
				get
				{
					return isBrowserSave;
				}
			}

	// Determine if this encoding can be displayed in a mail/news agent.
	public override bool IsMailNewsDisplay
			{
				get
				{
					return isMailNewsDisplay;
				}
			}

	// Determine if this encoding can be saved from a mail/news agent.
	public override bool IsMailNewsSave
			{
				get
				{
					return isMailNewsSave;
				}
			}

	// Get the IANA-preferred Web name for this encoding.
	public override String WebName
			{
				get
				{
					return webName;
				}
			}

	// Get the Windows code page represented by this object.
	public override int WindowsCodePage
			{
				get
				{
					return windowsCodePage;
				}
			}

#endif // !ECMA_COMPAT

}; // class ByteEncoding

}; // namespace I18N.Encoding
