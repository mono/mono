//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System.Text
{

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
	
}
