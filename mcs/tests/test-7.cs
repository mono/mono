using System;

namespace Mine {

	public class MyBoolean {
		public static implicit operator bool (MyBoolean x)
		{
			return true;
		}
	}

	public class MyTrueFalse {
		public static bool operator true (MyTrueFalse i)
		{
			return true;
		}
		
		public static bool operator false (MyTrueFalse i)
		{
			return false;
		}
	}
	
	public class Blah {

		public int i;

		public static int Main ()
		{
			Blah k, l;

			k = new Blah (2) + new Blah (3);
			if (k.i != 5)
				return 1;
			
			k = ~ new Blah (5);
			if (k.i != -6)
				return 1;
			
			
			k = + new Blah (4);
			if (k.i != 4)
				return 1;
			
			k = - new Blah (21);
			if (k.i != -21)
				return 1;

			k = new Blah (22) - new Blah (21);
			if (k.i != 1)
				return 1;

			if (!k)
				Console.WriteLine ("! returned true");

			int number = k;
			if (number != 1)
				return 1;
			
			k++;	
			++k;

			if (k)
				Console.WriteLine ("k is definitely true");

			k = new Blah (30);

			double f = (double) k;

			if (f != 30.0)
				return 1;

			int i = new Blah (5) * new Blah (10);

			if (i != 50)
				return 1;

			k = new Blah (50);
			l = new Blah (10);
			
			i = k / l;

			if (i != 5)
				return 1;

			i = k % l;

			if (i != 0)
				return 1;

			MyBoolean myb = new MyBoolean ();

			if (!myb)
				return 10;

			//
			// Tests the conditional operator invoking operator true
			MyTrueFalse mf = new MyTrueFalse ();
			int x = mf ? 1 : 2;
			if (x != 1)
				return 11;
			
			Console.WriteLine ("Test passed");
			return 0;
		}
	
		public Blah (int v)
		{
			i = v;
		}
	
		public static Blah operator + (Blah i, Blah j)
		{
			Blah b = new Blah (i.i + j.i);
			Console.WriteLine ("Overload binary + operator");
			return b;
		}

		public static Blah operator + (Blah i)
		{
			Console.WriteLine ("Overload unary + operator");
			return new Blah (i.i);
		}

		public static Blah operator - (Blah i)
		{
			Console.WriteLine ("Overloaded unary - operator");
			return new Blah (- i.i);
		}

		public static Blah operator - (Blah i, Blah j)
		{
			Blah b = new Blah (i.i - j.i);
			Console.WriteLine ("Overloaded binary - operator");
			return b;
		}

		public static int operator * (Blah i, Blah j)
		{
			Console.WriteLine ("Overloaded binary * operator");
			return i.i * j.i;
		}

		public static int operator / (Blah i, Blah j)
		{
			Console.WriteLine ("Overloaded binary / operator");
			return i.i / j.i;
		}

		public static int operator % (Blah i, Blah j)
		{
			Console.WriteLine ("Overloaded binary % operator");
			return i.i % j.i;
		}
		
		public static Blah operator ~ (Blah i)
		{
			Console.WriteLine ("Overloaded ~ operator");
			return new Blah (~i.i);
		}
	
		public static bool operator ! (Blah i)
		{
			Console.WriteLine ("Overloaded ! operator");
			return (i.i == 1);
		}

		public static Blah operator ++ (Blah i)
		{
			Blah b = new Blah (i.i + 1);
			Console.WriteLine ("Incrementing i");
			return b;
		}

		public static Blah operator -- (Blah i)
		{
			Blah b = new Blah (i.i - 1);
			Console.WriteLine ("Decrementing i");
			return b;
		}	
	
		public static bool operator true (Blah i)
		{
			Console.WriteLine ("Overloaded true operator");
			return (i.i == 3);
		}

		public static bool operator false (Blah i)
		{
			Console.WriteLine ("Overloaded false operator");
			return (i.i != 1);
		}	
	
		public static implicit operator int (Blah i) 
		{	
			Console.WriteLine ("Converting implicitly from Blah->int");
			return i.i;
		}

		public static explicit operator double (Blah i)
		{
			Console.WriteLine ("Converting explicitly from Blah->double");
			return (double) i.i;
		}

	}

}
