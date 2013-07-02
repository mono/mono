// Compiler options: -langversion:default

//
// Anonymous method group conversions
//

class X {
	delegate void T ();
	static event T Click;

	static void Method ()
	{
	}

	public static void Main ()
	{
		T t;

		// Method group assignment
		t = Method;

		Click += Method;
	}
}
