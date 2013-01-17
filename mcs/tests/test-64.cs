//
// This just test that we can compile this code.
//
// The challenge here is that LookupType needs to first look
// in classes defined in its class or parent classes before resorting
// to lookups in the namespace.
//

class Operator {
}

class Blah {

	public enum Operator { A, B };
	
	public Blah (Operator x)
	{
	}
}

class T {
	public static int Main ()
	{
		Blah b = new Blah (Blah.Operator.A);

		return 0;
	}
}

