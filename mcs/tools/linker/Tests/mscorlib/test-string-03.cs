using System;
using System.Text;

public class C
{
	public static unsafe int Main ()
	{
		string str = "abcd";
		fixed (char* ch = str) {
			var res1 = new string (ch);
			var res2 = new string (ch, 1, 1);
		}

		Encoding encoding = Encoding.ASCII;
		byte[] bytes = encoding.GetBytes (str);

		fixed (byte* bytePtr = bytes) {
			var res1 = new string ((sbyte*)bytePtr);
			var res2 = new string ((sbyte*)bytePtr, 0, 1);
			var res3 = new string ((sbyte*)bytePtr, 0, 1, encoding);
		}

		return 0;
	}
}