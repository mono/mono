using System;
class X {
	static void Main ()
	{

		P.x += 10;
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
		
