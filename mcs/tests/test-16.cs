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
			Console.WriteLine ("Blah->int");
			return 3;
		}

		public static implicit operator byte (Blah i)
		{
			Console.WriteLine ("Blah->byte");
			return 0;
		}
		
		public static implicit operator short (Blah i)
		{
			Console.WriteLine ("Blah->short");
			return 1;
		}
		
	}

	public class Foo : Blah {

		public static int Main ()
		{
			int number = new Foo () + new Foo () ;
			Console.WriteLine (number);

			Foo tmp = new Foo ();
			
			int k = tmp;

			Console.WriteLine ("Convert from Foo to float");
			float f = tmp;
			Console.WriteLine ("Converted");

			// The following will not work till we fix our UserCast::Emit
			// to convert the return value on the stack.
			if (f == 3)
				Console.WriteLine ("Best implicit conversion selected correctly.");

			Console.WriteLine ("F is {0}", f);

			if (number == 2 && k == 3)
				return 0;
			else
				return 1;
		}
	}
}
