// CS0841: A local variable `n' cannot be used before it is declared
// Line: 17

class MainClass
{
	public delegate void Fn (MainClass o);

	public static void Call (Fn f)
	{
		f(null);
	}

	public static void Main ()
	{
		Call (delegate (MainClass o) {
			n = o;
			MainClass n = new MainClass ();
		});
	}
}