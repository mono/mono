using System;

namespace Mine {

	public class Blah {

		public int i;

		public static void Main ()
		{
			Console.WriteLine ("Blah ");
			Blah k;

			k = new Blah () + new Blah (); 
			k = ~ new Blah ();
			k = + new Blah ();

			//int number = 5;
	
			//Console.WriteLine ("This is : " + number);
			
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

	}

}
