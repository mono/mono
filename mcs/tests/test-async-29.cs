using System;
using System.Threading.Tasks;

class C
{
	static async Task<int> Test()
	{
		await Task.Yield();
		await Task.Yield();
		await Task.Yield();
		return 1;
	} 
	
	public static int Main ()
	{
		Test ().Wait ();
		return 0;
	}
}
