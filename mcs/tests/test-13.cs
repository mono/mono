using System;

class Foo {

	public bool MyMethod ()
	{
		Console.WriteLine ("Base class method !");
		return true;
	}
}

class Blah : Foo {

	public static int Main ()
	{
		Blah k = new Blah ();

		Foo i = k;

		if (i.MyMethod ())
			return 0;
		else
			return 1;
			       

	}
	
}


	       
