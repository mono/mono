using globalA = A;

class A { }

class X {
	class A { }
	public static void Main ()
	{
		globalA a = new global::A ();
		System.Console.WriteLine (a.GetType ());
	}
}
