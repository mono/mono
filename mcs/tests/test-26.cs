using System;

public class Blah {

	public delegate int MyDelegate (int i, int j);
	
	public int Foo (int i, int j)
	{
		return i+j;
	}

	public static int Test1 ()
	{
		Blah f = new Blah ();

		MyDelegate del = new MyDelegate (f.Foo);

		MyDelegate another = new MyDelegate (del);

		int number = del (2, 3);

		int i = another (4, 6);
		
		Console.WriteLine ("Delegate invocation of one returned : " + number);

		Console.WriteLine ("Delegate invocation of the other returned : " + i);

		if (number == 5 && i == 10)
			return 0;
		else
			return 1;
	}
	
	public delegate int List (params int [] args);

	public static int Adder (params int [] args)
	{
		int total = 0;
		
		foreach (int i in args)
			total += i;

		return total;
	}

	public static int Test2 ()
	{
		List my_adder = new List (Adder);

		if (my_adder (1, 2, 3) != 6)
			return 2;

		return 0;
	}
	
	public static int Main ()
	{
		int v;

		v = Test1 ();
		if (v != 0)
			return v;

		v = Test2 ();
		if (v != 0)
			return v;

		Console.WriteLine ("All tests pass");
		return 0;
	}

}
