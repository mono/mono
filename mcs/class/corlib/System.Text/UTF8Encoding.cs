//
// System.Text.UTF8Encoding.cs
//
// Authors:
//   Sean MacIsaac (macisaac@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Text
{

	public class UTF8Encoding : Encoding
	{
		public override int GetByteCount(char[] chars, int index, int count)
		{
			// FIXME
			return count*6;
		}

		public override int GetBytes(char[] chars, int charIndex, int charCount, 
byte[] bytes, int byteIndex)
		{
			if (chars == null || bytes == null)
				throw new ArgumentNullException();

			if (charIndex < 0 || charCount < 0 || byteIndex < 0 ||
			    charIndex + charCount > chars.Length ||
			    byteIndex + GetByteCount(chars, charIndex, charCount) > bytes.Length)
				throw new ArgumentOutOfRangeException();

			// this is slow implementation just to get the things going
			int outputIndex = byteIndex;
			for (int i = 0; i < charCount; i++)
			{
				int charCode = (int)chars[charIndex + i];

				if (charCode < 0)  // negative chars are invalid
					throw new ArgumentOutOfRangeException();

				if (charCode < 0x80)
					bytes [outputIndex++] = (byte)charCode;
				else
					if (charCode < 0x800)
					{
						bytes [outputIndex++] = (byte)((charCode >> 6) | 0xC0);
						bytes [outputIndex++] = (byte)((charCode & 0x3F) | 0x80);
					}
					else
					{
						// LAME: if chars[] come as UTF-16 - here we have to decode the surrogate pair, before proceeding
						// charCode = some magic with charCode and (int)chars[++i + charIndex] if needed
						if (charCode < 0x10000)
						{
							bytes [outputIndex++] = (byte)((charCode >> 12) | 0xE0);
							bytes [outputIndex++] = (byte)(((charCode >> 6) & 0x3F) | 0x80);
							bytes [outputIndex++] = (byte)((charCode & 0x3F) | 0x80);
						}
						else
							if (charCode < 0x200000)
							{
								bytes [outputIndex++] = (byte)((charCode >> 18) | 0xF0);
								bytes [outputIndex++] = (byte)(((charCode >> 12) & 0x3F) | 0x80);
								bytes [outputIndex++] = (byte)(((charCode >> 6) & 0x3F) | 0x80);
								bytes [outputIndex++] = (byte)((charCode & 0x3F) | 0x80);
							}
							else
								if (charCode < 0x4000000)
								{
									bytes [outputIndex++] = (byte)((charCode >> 24) | 0xF8);
									bytes [outputIndex++] = (byte)(((charCode >> 18) & 0x3F) | 0x80);
									bytes [outputIndex++] = (byte)(((charCode >> 12) & 0x3F) | 0x80);
									bytes [outputIndex++] = (byte)(((charCode >> 6) & 0x3F) | 0x80);
									bytes [outputIndex++] = (byte)((charCode & 0x3F) | 0x80);
								}
								else
								{
									bytes [outputIndex++] = (byte)((charCode >> 30) | 0xFC);
									bytes [outputIndex++] = (byte)(((charCode >> 24) & 0x3F) | 0x80);
									bytes [outputIndex++] = (byte)(((charCode >> 18) & 0x3F) | 0x80);
									bytes [outputIndex++] = (byte)(((charCode >> 12) & 0x3F) | 0x80);
									bytes [outputIndex++] = (byte)(((charCode >> 6) & 0x3F) | 0x80);
									bytes [outputIndex++] = (byte)((charCode & 0x3F) | 0x80);
								}
					}
			}

			return (outputIndex - byteIndex);
		}

		public override int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
				char[] chars = s.ToCharArray(charIndex, charCount);
				return 	GetBytes(chars, 0, charCount, bytes, byteIndex);
		}

		public override byte[] GetBytes(string s)
		{
			char[] chars = s.ToCharArray();
			byte[] bytes = new byte[GetByteCount(chars, 0, chars.Length)];

			GetBytes(chars, 0, chars.Length, bytes, bytes.Length);

			return bytes;
		}

		public override int GetCharCount(byte[] bytes, int byteIndex, int 
byteCount)
		{
			int count = 0;
			for (int i = byteIndex; i < byteIndex + byteCount; i++)
				if ((bytes[i] & 0xC0) != 0x80)
					count++;

			return count;
		}

		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, 
char[] chars, int charIndex)
		{
			if (chars == null || bytes == null)
				throw new ArgumentNullException();

			if (charIndex < 0 || byteCount < 0 || byteIndex < 0 ||
			    charIndex + GetCharCount(bytes, byteIndex, byteCount) > chars.Length 
||
			    byteIndex + byteCount > bytes.Length)
				throw new ArgumentOutOfRangeException();

			// FIXME
			return 0;
		}

		public override int GetMaxByteCount(int charCount)
		{
			return charCount*6;
		}

		public override int GetMaxCharCount(int byteCount)
		{
			return byteCount;
		}
	}
}
