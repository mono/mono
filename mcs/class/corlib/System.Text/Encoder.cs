//
// System.Text.Encoder.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System.Text
{

	[Serializable]
	public abstract class Encoder
	{

		protected Encoder()
		{
			// fixme: dont know what do do here
		}

		public abstract int GetByteCount (char[] chars, int index, int count, bool flush);

		public abstract int GetBytes (char[] chars, int charIndex, int charCount,
					      byte[] bytes, int byteIndex, bool flush);
	}

	internal class DefaultEncoder : Encoder {

		public Encoding encoding;
		
		public DefaultEncoder (Encoding enc)
		{
			encoding = enc;
		}

		public override int GetByteCount (char[] chars, int index, int count, bool flush)
		{
			return encoding.GetByteCount (chars, index, count);
		}

		public override int GetBytes (char[] chars, int charIndex, int charCount,
					      byte[] bytes, int byteIndex, bool flush)
		{
			return encoding.GetBytes (chars, charIndex, charCount, bytes, byteIndex);
		}

	}
	
	internal class IConvEncoder : Encoder {

		private IntPtr converter;

		public IConvEncoder (string name, bool big_endian)
		{
			converter = Encoding.IConvNewEncoder (name, big_endian);
		}

		public override int GetByteCount (char[] chars, int index, int count, bool flush)
		{
			if (chars == null)
				throw new ArgumentNullException ();

			if (index + count > chars.Length)
				throw new ArgumentOutOfRangeException ();

			int res = Encoding.IConvGetByteCount (converter, chars, index, count);
			
			if (flush)
				Encoding.IConvReset (converter);

			return res;
		}

		public override int GetBytes (char[] chars, int charIndex, int charCount,
					      byte[] bytes, int byteIndex, bool flush)
		{
			if ((chars == null) || (bytes == null))
				throw new ArgumentNullException ();

			if ((charIndex < 0) || (charCount < 0) || (byteIndex < 0))
				throw new ArgumentOutOfRangeException ();

			if (charIndex + charCount > chars.Length)
				throw new ArgumentOutOfRangeException ();

			if (byteIndex + charCount > bytes.Length)
				throw new ArgumentOutOfRangeException ();

			int res = Encoding.IConvGetBytes (converter, chars, charIndex, charCount,
							  bytes, byteIndex);
			
			if (flush)
				Encoding.IConvReset (converter);

			return res;
		}
	}
}
