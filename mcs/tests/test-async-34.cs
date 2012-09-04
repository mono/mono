using System;
using System.Threading.Tasks;

class C
{
	static int called;
	
	public static async Task Test (bool arg)
	{
		if (arg)
			return;
		
		called++;
		await Task.FromResult (1);
	}
	
	
	public static async Task Test2 (bool arg)
	{
		if (arg)
			return;
		
		called++;
	}

	public static int Main ()
	{
		Test (true).Wait ();
		if (called != 0)
			return 1;
		
		Test2 (true).Wait ();
		if (called != 0)
			return 2;
		
		return 0;
	}
}