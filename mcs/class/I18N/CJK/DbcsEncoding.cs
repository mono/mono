//
// I18N.CJK.DbcsEncoding
//
// Author:
//   Alan Tam Siu Lung (Tam@SiuLung.com)
//

using System;
using System.Text;
using I18N.Common;

namespace I18N.CJK
{
	internal class DbcsEncoding : Encoding
	{
		internal DbcsConvert convert;
		
		public DbcsEncoding(int codePage) : base(codePage) {}
		
		// Get the number of bytes needed to encode a character buffer.
		public override int GetByteCount(char[] chars, int index, int count)
		{
			if (chars == null)
				throw new ArgumentNullException("chars");
			if (index < 0 || index > chars.Length)
				throw new ArgumentOutOfRangeException("index", Strings.GetString("ArgRange_Array"));
			if (count < 0 || index + count > chars.Length)
				throw new ArgumentOutOfRangeException("count", Strings.GetString("ArgRange_Array"));
			byte[] buffer = new byte[count * 2];
			return GetBytes(chars, index, count, buffer, 0);
		}
		
		// Get the bytes that result from encoding a character buffer.
		public override int GetBytes(char[] chars, int charIndex, int charCount,
					     byte[] bytes, int byteIndex)
		{
			if (chars == null)
				throw new ArgumentNullException("chars");
			if (bytes == null)
				throw new ArgumentNullException("bytes");
			if (charIndex < 0 || charIndex > chars.Length)
				throw new ArgumentOutOfRangeException("charIndex", Strings.GetString("ArgRange_Array"));
			if (charCount < 0 || charIndex + charCount > chars.Length)
				throw new ArgumentOutOfRangeException("charCount", Strings.GetString("ArgRange_Array"));
			if (byteIndex < 0 || byteIndex > bytes.Length)
				throw new ArgumentOutOfRangeException("byteIndex", Strings.GetString("ArgRange_Array"));
			return 0; // For subclasses to implement
		}
		
		// Get the number of characters needed to decode a byte buffer.
		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			if (bytes == null)
				throw new ArgumentNullException("bytes");
			if (index < 0 || index > bytes.Length)
				throw new ArgumentOutOfRangeException("index", Strings.GetString("ArgRange_Array"));
			if (count < 0 || index + count > bytes.Length)
				throw new ArgumentOutOfRangeException("count", Strings.GetString("ArgRange_Array"));
			char[] buffer = new char[count];
			return GetChars(bytes, index, count, buffer, 0);
		}
		
		// Get the characters that result from decoding a byte buffer.
		public override int GetChars(byte[] bytes, int byteIndex, int byteCount,
					     char[] chars, int charIndex)
		{
			if (bytes == null)
				throw new ArgumentNullException("bytes");
			if (chars == null)
				throw new ArgumentNullException("chars");
			if (byteIndex < 0 || byteIndex > bytes.Length)
				throw new ArgumentOutOfRangeException("byteIndex", Strings.GetString("ArgRange_Array"));
			if (byteCount < 0 || byteIndex + byteCount > bytes.Length)
				throw new ArgumentOutOfRangeException("byteCount", Strings.GetString("ArgRange_Array"));
			if (charIndex < 0 || charIndex > chars.Length)
				throw new ArgumentOutOfRangeException("charIndex", Strings.GetString("ArgRange_Array"));
			return 0; // For subclasses to implement
		}
		
		// Get the maximum number of bytes needed to encode a
		// specified number of characters.
		public override int GetMaxByteCount(int charCount)
		{
			if (charCount < 0)
				throw new ArgumentOutOfRangeException("charCount", Strings.GetString("ArgRange_NonNegative"));
			return charCount * 2;
		}
		
		// Get the maximum number of characters needed to decode a
		// specified number of bytes.
		public override int GetMaxCharCount(int byteCount)
		{
			if (byteCount < 0) {
				throw new ArgumentOutOfRangeException("byteCount", Strings.GetString("ArgRange_NonNegative"));
			}
			return byteCount;
		}
		
		// Get a decoder that handles a rolling state.
		public override Decoder GetDecoder()
		{
			return new DbcsDecoder(convert);
		}
		
		// Determine if this encoding can be displayed in a Web browser.
		public override bool IsBrowserDisplay
		{
			get { return true; }
		}
		
		// Determine if this encoding can be saved from a Web browser.
		public override bool IsBrowserSave
		{
			get { return true; }
		}
		
		// Determine if this encoding can be displayed in a mail/news agent.
		public override bool IsMailNewsDisplay
		{
			get { return true; }
		}
		
		// Determine if this encoding can be saved from a mail/news agent.
		public override bool IsMailNewsSave
		{
			get { return true; }
		}
		
		// Decoder that handles a rolling state.
		internal class DbcsDecoder : Decoder
		{
			private DbcsConvert convert;
			internal int lastByte;
			
			// Constructor.
			public DbcsDecoder(DbcsConvert convert)
			{
				this.convert = convert;
				this.lastByte = 0;
			}
			
			// Override inherited methods.
			public override int GetCharCount(byte[] bytes, int index, int count)
			{
				if (bytes == null)
					throw new ArgumentNullException("bytes");
				if (index < 0 || index > bytes.Length)
					throw new ArgumentOutOfRangeException("index", Strings.GetString("ArgRange_Array"));
				if (count < 0 || count > (bytes.Length - index))
					throw new ArgumentOutOfRangeException("count", Strings.GetString("ArgRange_Array"));
				char[] buffer = new char[count * 2];
				return GetChars(bytes, index, count, buffer, 0);
			}
			
			public override int GetChars(byte[] bytes, int byteIndex, int byteCount,
						     char[] chars, int charIndex)
			{
				if (bytes == null)
					throw new ArgumentNullException("bytes");
				if (chars == null)
					throw new ArgumentNullException("chars");
				if (byteIndex < 0 || byteIndex > bytes.Length)
					throw new ArgumentOutOfRangeException("byteIndex", Strings.GetString("ArgRange_Array"));
				if (byteCount < 0 || byteIndex + byteCount > bytes.Length)
					throw new ArgumentOutOfRangeException("byteCount", Strings.GetString("ArgRange_Array"));
				if (charIndex < 0 || charIndex > chars.Length)
					throw new ArgumentOutOfRangeException("charIndex", Strings.GetString("ArgRange_Array"));
				return 0; // For subclasses to implement
			}
		}
	}
}
