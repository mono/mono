using System;
        
delegate void A ();

class DelegateTest {
	public static void Main (string[] argv)
	{
		Console.WriteLine ("Test");

		foreach (string arg in argv) {
			Console.WriteLine ("OUT: {0}", arg);
			A a = delegate {
				Console.WriteLine ("arg: {0}", arg);
			};
			a ();
		}
	}
}
      
