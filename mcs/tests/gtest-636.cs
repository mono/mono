using System;

class MainClass
{
	public static int Main ()
	{
		var res = XX () | YY();
		if (!res.Value)
			return 1;

		if (xx != 1)
			return 2;

		if (yy != 1)
			return 2;

		return 0;
	}

	static int xx;
	static bool XX ()
	{
		++xx;
		Console.WriteLine ("XX");
		return true;
	}	

	static int yy;
	static bool? YY ()
	{
		if (xx == 1)
			++yy;
			
		Console.WriteLine ("YY");
		return true;
	}	
}
