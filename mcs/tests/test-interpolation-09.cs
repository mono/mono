using System;

class EscapedQuotedInterpolatedFormatSpecifier
{
	public static int Main ()
	{
		string ss = "ss";
		var t = $@"\4{ss:\u007B}\5";

		Console.WriteLine (t);
		if (t != @"\4ss\5")
			return 1;

		return 0;
	}
}
