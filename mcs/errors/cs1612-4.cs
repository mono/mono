// CS1612: Cannot modify a value type return value of `X.P'. Consider storing the value in a temporary variable
// Line: 9

using System;
class X {
	static void Main ()
	{

		bar (out P.x);
		Console.WriteLine ("Got: " + P.x);
	}

	static void bar (out int x) { x = 10; }

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
		
