using System;

namespace Mine {

	public class Blah {

		public int i;

		public Blah ()
		{
			Console.WriteLine ("Inside the constructor now");
		}
	
		public static int Main ()
		{
			Blah k;

			k = new Blah () + new Blah (); 
			k = ~ new Blah ();
			k = + new Blah ();

			if (!k)
				Console.WriteLine ("Overloaded ! operator returned true");

			int number = k;
			Console.WriteLine (number);
			
			k = 5;		

			k++;	
			++k;

			if (k)
				Console.WriteLine ("k is definitely true");

			k = new Blah ();

			double f = (double) k;

			if (f == 2.0)
				Console.WriteLine ("Explicit conversion correct.");

			return 0;
			
		}
		
		public static Blah operator + (Blah i, Blah j)
		{
			Console.WriteLine ("Overloaded binary + operator");
			return null; 
		}

		public static Blah operator + (Blah i)
		{
			Console.WriteLine ("Overloaded unary + operator");
			return null;
		}
		
	
		public static Blah operator ~ (Blah i)
		{
			Console.WriteLine ("Overloaded ~ operator");
			return null;
		}
	
		public static bool operator ! (Blah i)
		{
			Console.WriteLine ("Overloaded ! operator");
			return true;
		}

		public static Blah operator ++ (Blah i)
		{
			Console.WriteLine ("Incrementing i");
			return null;
		}

		public static Blah operator -- (Blah i)
		{
			Console.WriteLine ("Decrementing i");
			return null;
		}	
	
		public static bool operator true (Blah i)
		{
			Console.WriteLine ("Overloaded true operator");
			return true;
		}

		public static bool operator false (Blah i)
		{
			Console.WriteLine ("Overloaded false operator");
			return false;
		}	
	
		public static implicit operator int (Blah i) 
		{	
			Console.WriteLine ("Converting implicitly from Blah->int");
			return 3;
		}

		public static implicit operator Blah (int i)
		{
			Console.WriteLine ("Converting implicitly from int->Blah");
			return null;
		}

		public static explicit operator double (Blah i)
		{
			Console.WriteLine ("Converting explicitly from Blah->double");
			return 2.0;
		}

	}

}
