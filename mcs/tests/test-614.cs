using System;

class C {

	public static int	value;
		
	static internal void And ()
	{	
		if ((false & (value++ == 1)) != (false & (++value == 1)))
			return;
		
		if (((value++ == 1) & false) != ((++value == 1) & false))
			return;
		
		if ((false && (value++ == 1)) != (false && (++value == 1)))
			return;
		
		if (((value++ == 1) && false) != ((++value == 1) && false))
			return;
	}

	static internal void Or ()
	{	
		if ((false | (value++ == 1)) != (false | (++value == 1)))
			return;
		
		if (((value++ == 1) | false) != ((++value == 1) | false))
			return;
		
		if ((true || (value++ == 1)) != (true || (++value == 1)))
			return;
		
		if (((value++ == 1) || true) != ((++value == 1) || true))
			return;
	}
	
	public static int Main ()
	{
		value = 0;
		And ();
		Console.WriteLine (value);
		if (value != 6)
			return 1;
		
		value = 0;
		Or ();
		Console.WriteLine (value);
		if (value != 6)
			return 2;
			
		return 0;
	}
}
