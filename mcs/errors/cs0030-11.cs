// CS0030: Cannot convert type `string' to `IA'
// Line: 13

interface IA
{
}

class MainClass
{
	public static void Main ()
	{
		string s = "s";
		IA i = (IA) s;
	}
}
