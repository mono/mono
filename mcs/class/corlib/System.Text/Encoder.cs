//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System.Text
{

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
	
}
