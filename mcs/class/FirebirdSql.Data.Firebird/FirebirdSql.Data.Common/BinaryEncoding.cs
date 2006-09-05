/*
 *  BinaryEncoding handler for .Net.  This class implements
 *	a symmetric encoding that will convert string to byte[]
 *  and byte[] to string without any character set
 *  transliteration.
 *
 *  The contents of this file were written by jimb
 *  at connectedsw.com on Dec 9, 2004.  It is placed in
 *  the Public Domain and may be used as you see fit.
 */

using System;
using System.Text;

namespace FirebirdSql.Data.Common
{
	internal class BinaryEncoding : Encoding
	{
		#region Static Methods

		public static string BytesToString(byte[] byteArray)
		{
			// This code isn't great because it requires a double copy,
			// but it requires unsafe code to solve the problem efficiently.
			char[] charArray = new char[byteArray.GetLength(0)];
			Array.Copy(byteArray, charArray, byteArray.Length);

			return new string(charArray);
		}

		static void Validate(object data, int dataLength, int index, int count)
		{
			if (data == null)
			{
				throw new ArgumentNullException();
			}

			if (index < 0 || count < 0 || dataLength - index < count)
			{
				throw new ArgumentOutOfRangeException();
			}
		}

		#endregion

		#region Methods

		public override int GetByteCount(char[] chars, int index, int count)
		{
			Validate(chars, chars.Length, index, count);

			return count;
		}

		public override int GetByteCount(string chars)
		{
			return chars.Length;
		}

		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int index)
		{
			Validate(chars, chars.Length, charIndex, charCount);

			if (index < 0 || index > bytes.Length)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (bytes.Length - index < charCount)
			{
				throw new ArgumentException();
			}

			int charEnd = charIndex + charCount;
			while (charIndex < charEnd)
			{
				bytes[index++] = (byte)chars[charIndex++];
			}

			return charCount;
		}

		public override int GetBytes(string chars, int charIndex, int charCount, byte[] bytes, int index)
		{
			Validate(chars, chars.Length, charIndex, charCount);

			if (index < 0 || index > bytes.Length)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (bytes.Length - index < charCount)
			{
				throw new ArgumentException();
			}

			int charEnd = charIndex + charCount;
			while (charIndex < charEnd)
			{
				bytes[index++] = (byte)chars[charIndex++];
			}

			return charCount;
		}

		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			Validate(bytes, bytes.Length, index, count);

			return (count);
		}

		public override int GetChars(byte[] bytes, int index, int count, char[] chars, int charIndex)
		{
			Validate(bytes, bytes.Length, index, count);

			if (charIndex < 0 || charIndex > chars.Length)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (chars.Length - charIndex < count)
			{
				throw new ArgumentException();
			}

			int byteEnd = index + count;
			while (index < byteEnd)
			{
				chars[charIndex++] = (char)bytes[index++];
			}

			return count;
		}

#if (!NETCF)
	
        public override string GetString(byte[] bytes)
		{
			return BytesToString(bytes);
		}

#endif

		public override string GetString(byte[] bytes, int index, int count)
		{
			Validate(bytes, bytes.Length, index, count);

			return BytesToString(bytes);
		}

		public override int GetMaxByteCount(int charCount)
		{
			return charCount;
		}

		public override int GetMaxCharCount(int count)
		{
			return count;
		}

		#endregion
	}
}
