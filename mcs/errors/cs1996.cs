// CS1996: The `await' operator cannot be used in the body of a lock statement
// Line: 12

using System;
using System.Threading.Tasks;

class C
{
	public async Task Test ()
	{
		lock (this) {
			await Call ();
		}
	}
	
	static Task Call ()
	{
		return null;
	}
}
