using System;

namespace Mine {

	public class Blah {

		public int i;

		public static int Main ()
		{
			Console.WriteLine ("Blah ");
			Blah k;

			k = new Blah () + new Blah (); 
			k = ~ new Blah ();
			k = + new Blah ();

			int number = k;
			Console.WriteLine (number);
			
			// Uncomment this to see how beautifully we catch errors :)
			// Console.WriteLine (k);
			
			//Console.WriteLine ("This is : " + number);
	
			return 0;
			
		}
		
		public static Blah operator + (Blah i, Blah j)
		{
			Console.WriteLine ("Wooo!");
			return null; 
		}

		public static Blah operator + (Blah i)
		{
			Console.WriteLine ("This is the unary one !");
			return null;
		}
		
	
		public static Blah operator ~ (Blah i)
		{
			Console.WriteLine ("This is even better !");
			return null;
		}
	
		public static implicit operator int (Blah i) 
		{	
			Console.WriteLine ("User-defined implicit conversion works !");
			return 3;
		}

	}

}
