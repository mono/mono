using System;

public class Blah {

	public delegate int MyDelegate (int i, int j);
	
	public int Foo (int i, int j)
	{
		return i+j;
	}

	public static int Main ()
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

}
