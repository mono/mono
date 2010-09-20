using System;

class C
{
	public static int Main ()
	{
		const string s = "oups";
		if (s.Length != 4) {
			Console.WriteLine (s.Length);
			return 2;
		}
		
		return 0;
	}
}
