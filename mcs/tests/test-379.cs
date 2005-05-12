using System;

public class DeadCode {

	public static void Main ()
	{
		SomeFunc ("...");
	}

	static public string SomeFunc (string str)
	{
		return str;
		int i = 0, pos = 0;
	}

}
