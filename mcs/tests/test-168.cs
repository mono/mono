using System;

struct X {
    static public implicit operator string (X x)
    {
       return "x";
    }

}

class Test { 

	static public int Main ()
	{
		X x = new X ();
		Console.WriteLine (x);
	
		return 0;
	}
}

