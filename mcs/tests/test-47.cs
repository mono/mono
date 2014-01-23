//
// Short-circuit evaluation tests
//
using System;

class X {

	static int t_count = 0;
	static int f_count = 0;

	static bool f ()
	{
		Console.WriteLine ("f");
		f_count++;
		return false;
	}

	static bool t ()
	{
		Console.WriteLine ("t");
		t_count++;
		return true;
	}			
	
	public static int Main ()
	{
		if (t () && t ()){
			f_count--;
		}
		
		if (t_count != 2)
			return 1;

		if (f_count != -1)
			return 3;

		f_count = 0;

		if (t () && f ())
			if (t_count != 3 && f_count == 1)
				return 2;

		if (f () && f ())
			return 3;

		if (f_count != 2)
			return 4;

		if (f () && t ())
			return 5;

		if (f_count != 3)
			return 6;

		if (t_count != 3)
			return 7;

		//
		// reset
		//
		Console.WriteLine ("or");
		
		t_count = f_count = 0;

		if (t () || t ()){
			if (t_count != 1)
				return 8;
		} else
			return 9;

		if (t () || f ()){
			if (f_count != 0)
				return 10;
			if (t_count != 2)
				return 16;
		} else
			return 11;
		
		if (f () || f ()){
			return 12;
		} else
			if (f_count != 2)
				return 13;
		
		if (f () || t ()){
			if (f_count != 3)
				return 15;
			if (t_count != 3)
				return 17;
		} else
			return 14;
			
		return 0;
	}
}
