using System;

delegate void D1 ();
delegate void D2 ();

public class DelegateTest {
	static void Foo (D1 d)
	{
		d ();
	}
	
	static void Foo (D2 d)
	{
	}

	static int counter = 99;
	public static int Main ()
	{
		Foo (new D1 (delegate {
			counter = 82;
			Console.WriteLine ("In");
		 }));
		 
		 if (counter != 82)
			 return 1;
		 
		 return 0;
	 }
}


