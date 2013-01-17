// CS1985: The `await' operator cannot be used in the body of a catch clause
// Line: 13

using System;
using System.Threading.Tasks;

class C
{
	public async Task Test ()
	{
		try {
		} catch {
			await Call ();
		}
	}
	
	static Task Call ()
	{
		return null;
	}
}
