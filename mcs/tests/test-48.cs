using System;

public class Blah {
	
	public const int i = 5;

	public static int Main ()
	{
		const int foo = 10;
		
		Blah blah = new Blah ();
		int j = Blah.i;

		if (j != 5)
			return 1;

		if (foo != 10)
			return 1;

		for (int i = 0; i < 5; ++i){
			const int bar = 15;

			Console.WriteLine (bar);
			Console.WriteLine (foo);
		}

		Console.WriteLine ("Constant emission test okay");

		return 0;
	}
}
