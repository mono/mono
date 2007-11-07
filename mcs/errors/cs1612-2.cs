// CS1612: Cannot modify a value type return value of `X.P'. Consider storing the value in a temporary variable
// Line: 9

using System;
class X {
	static void Main ()
	{

		P.x = 10;
		Console.WriteLine ("Got: " + P.x);
	}

	static G P {
	 get {
		return g;
	 }
	}

	static G g = new G ();

	struct G {
		public int x;
	}
}
		
