using System;

public class Blah {

	public delegate int MyDelegate (int i, int j);
	
	public int Foo (int i, int j)
	{
		return i+j;
	}

	public static int Main ()
	{
		Blah i = new Blah ();

		MyDelegate del = new MyDelegate (i.Foo);

		int number = del (2, 3);

		if (number == 5)
			return 0;
		else
			return 1;

	}

}
