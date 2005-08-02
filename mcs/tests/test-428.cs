using globalA = A;

class A { }

class X {
	class A { }
	static void Main ()
	{
		globalA a = new global::A ();
		System.Console.WriteLine (a.GetType ());
	}
}
