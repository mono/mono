//
// System.Text.ASCIIEncoding.cs
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Text {
        
	public class ASCIIEncoding : Encoding
	{
		public ASCIIEncoding () : base ()
		{
			encoding_name = "US-ASCII";
			body_name = "us-ascii";
			header_name = "us-ascii";
			web_name = "us-ascii";
			is_browser_display = false;
			is_browser_save = false;
			is_mail_news_display = true;
			is_mail_news_save = true;
		}

		public override int GetByteCount (string chars)
		{
			if (chars == null) 
				throw new ArgumentNullException ();

			return chars.Length;
		}

		public override int GetByteCount (char[] chars)
		{
			if (chars == null) 
				throw new ArgumentNullException ();

			return chars.Length;
		}

		public override int GetByteCount (char[] chars, int index, int count)
		{
			if (chars == null) 
				throw new ArgumentNullException ();

			if ((index < 0) || (count <= 0) || ((index + count) >= chars.Length))
				throw new ArgumentOutOfRangeException ();

			return count;
		}

		public override int GetBytes (char[] chars, int charIndex, int charCount,
					      byte[] bytes, int byteIndex)
		{
			if ((bytes == null) || (chars == null))
				throw new ArgumentNullException ();

			if ((byteIndex < 0) || (charIndex < 0) || (charCount < 0) ||
			    ((charIndex + charCount) > chars.Length) ||
			    (byteIndex >= bytes.Length))
				throw new ArgumentOutOfRangeException ();

			if ((bytes.Length - byteIndex) < charCount)
				throw new ArgumentException ();

			for (int i = 0; i < charCount; i++)
				if (chars[charIndex+i] > 0x7f)
					bytes[byteIndex+i] = (byte) '?';
				else
					bytes[byteIndex+i] = (byte) chars[charIndex+i];

			return charCount;
		}

		public override int GetBytes (string chars, int charIndex, int charCount,
					      byte[] bytes, int byteIndex)
		{
			return GetBytes (chars.ToCharArray (), charIndex, charCount,
					 bytes, byteIndex);
		}

		public override int GetCharCount (byte[] bytes)
		{
			if (bytes == null) 
				throw new ArgumentNullException ();

			return bytes.Length;
		}

		public override int GetCharCount (byte[] bytes, int index, int count)
		{
			if (bytes == null) 
				throw new ArgumentNullException ();

			if ((index < 0) || (count <= 0) || ((index + count) >= bytes.Length))
				throw new ArgumentOutOfRangeException ();

			return count;
		}

		public override int GetChars (byte[] bytes, int byteIndex, int byteCount,
					      char[] chars, int charIndex)
		{
			if ((bytes == null) || (chars == null))
				throw new ArgumentNullException ();

			if ((byteIndex < 0) || (charIndex < 0) || (byteCount < 0) ||
			    ((byteIndex + byteCount) > bytes.Length) ||
			    (charIndex >= chars.Length))
				throw new ArgumentOutOfRangeException ();

			if ((chars.Length - charIndex) < byteCount)
				throw new ArgumentException ();

			for (int i = 0; i < byteCount; i++)
				if (bytes[byteIndex+i] > 0x7f)
					chars[charIndex+i] = '?';
				else
					chars[charIndex+i] = (char) bytes[byteIndex+i];

			return byteCount;
		}

		public override int GetMaxByteCount (int charCount)
		{
			if (charCount < 0) 
				throw new ArgumentOutOfRangeException ();

			return charCount;
		}

		public override int GetMaxCharCount (int byteCount)
		{
			if (byteCount < 0) 
				throw new ArgumentOutOfRangeException ();

			return byteCount;
		}

		public override string GetString (byte[] bytes)
		{
			if (bytes == null) 
				throw new ArgumentNullException ();

			return new String (GetChars (bytes, 0, bytes.Length));
		}

		public override string GetString (byte[] bytes, int byteIndex, int byteCount)
		{
			if (bytes == null) 
				throw new ArgumentNullException ();

			if ((byteIndex < 0) || (byteCount <= 0) || 
			    ((byteIndex + byteCount) >= bytes.Length))
				throw new ArgumentOutOfRangeException ();

			return new String (GetChars (bytes, byteIndex, byteCount));
		}

	}
}

