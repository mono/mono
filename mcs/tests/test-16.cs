using System;

namespace Mine {

	public class Blah {

		public static int operator + (Blah i, Blah j)
		{
			Console.WriteLine ("Base class binary + operator");
			return 2; 
		}

		public static implicit operator int (Blah i)
		{
			Console.WriteLine ("Converting from Blah->int");
			return 3;
		}
	}

	public class Foo : Blah {

		public static int Main ()
		{
			int number = new Foo () + new Foo () ;
			Console.WriteLine (number);

			int k = new Foo ();

			if (number == 2 && k == 3)
				return 0;
			else
				return 1;
		}
	}
}
