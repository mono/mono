public enum Modifiers
{
	Public = 0x0001
}

class Foo
{
	internal Modifiers Modifiers {
		get {
			return Modifiers.Public;
		}
	}
}

class Bar
{
	public static int Main ()
	{
		System.Console.WriteLine (Modifiers.Public);
		return 0;
	}
}
