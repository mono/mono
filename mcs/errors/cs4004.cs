// CS4004: The `await' operator cannot be used in an unsafe context
// Line: 12
// Compiler options: -unsafe

using System;
using System.Threading.Tasks;

class C
{
	public async Task Test ()
	{
		unsafe {
			await Call ();
		}
	}
	
	static Task Call ()
	{
		return null;
	}
}
