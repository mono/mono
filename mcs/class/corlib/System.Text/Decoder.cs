//
// System.Text.Decoder.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System.Text
{

	[Serializable]
	public abstract class Decoder
	{
		
		protected Decoder ()
		{
			// fixme: dont know what do do here
		}

		public abstract int GetCharCount (byte[] bytes, int index, int count);

		public abstract int GetChars (byte[] bytes, int byteIndex, int byteCount,
					      char[] chars, int charIndex);
	}

	internal class DefaultDecoder : Decoder {

		public Encoding encoding;

		public DefaultDecoder (Encoding enc)
		{
			encoding = enc;
		}

		public override int GetCharCount (byte[] bytes, int index, int count)
		{
			return encoding.GetCharCount (bytes, index, count);
		}

		public override int GetChars (byte[] bytes, int byteIndex, int byteCount,
					      char[] chars, int charIndex)
		{
			return encoding.GetChars (bytes, byteIndex, byteCount, chars, charIndex);
		}

	}
	
	internal class IConvDecoder : Decoder {
		
		private IntPtr converter;

		public IConvDecoder (string name, bool big_endian)
		{
			converter = Encoding.IConvNewDecoder (name, big_endian);
		}

		public override int GetCharCount (byte[] bytes, int index, int count)
		{
			if (bytes == null)
				throw new ArgumentNullException ();

			if (index + count > bytes.Length)
				throw new ArgumentOutOfRangeException ();

			return Encoding.IConvGetCharCount (converter, bytes, index, count);
		}

		public override int GetChars (byte[] bytes, int byteIndex, int byteCount,
					      char[] chars, int charIndex)
		{
			if ((bytes == null) || (chars == null))
				throw new ArgumentNullException ();

			if ((byteIndex < 0) || (byteCount < 0) || (charIndex < 0))
				throw new ArgumentOutOfRangeException ();

			if (byteIndex + byteCount > bytes.Length)
				throw new ArgumentOutOfRangeException ();

			if (charIndex > chars.Length)
				throw new ArgumentOutOfRangeException ();

			return Encoding.IConvGetChars (converter, bytes, byteIndex, byteCount,
						       chars, charIndex);
		}
	}	
}
