class A
{
	public A this [string arg] {
		get {
			return new A ();
		}
		set {
		}
	}

	public int Count (string a)
	{
		return 1;
	}
}

static class B
{
	public static string Count (this A arg)
	{
		return "x";
	}
}

class X
{
	public static void Main ()
	{
		var a = new A ();
		var b = a ["b"]?.Count ();
		System.Console.WriteLine (b);
	}
}