using System;
using System.Collections;
using System.Threading.Tasks;

class C
{
	static async Task<int> Test ()
	{
		var t = new Task<int> (() => { throw new ApplicationException ();});
		try {
			try {
				t.Start ();
				await t;
			} catch {
				throw;
			}
			return -1;
		} catch {
			return 1;
		}   
	}

	public static int Main ()
	{
		var res = Test ().Result;
		if (res != 1)
			return 1;
		
		return 0;
	}
}
