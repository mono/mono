using System;

class MainClass
{
	public static implicit operator string (MainClass src)
	{
		return null;
	}

	public static int Main ()
	{
		var obj = new MainClass ();
		var s = "x";
		var res = (string) obj ?? s;
		if (res != "x")
			return 1;

		return 0;
	}
}
