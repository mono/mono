using System;

class X {
	static int i;
	static int j;
	
	static void m ()
	{
		i = 0;
		j = 0;
		
		try {
			throw new ArgumentException ("Blah");
		} catch (ArgumentException){
			i = 1;
		} catch (Exception){
			i = 2;
		} finally {
			j = 1;
		}
	}

	static int Main ()
	{
		m ();
		if (i != 1)
			return 1;
		if (j != 1)
			return 2;

		return 0;
	}
}
