// CS4004: The `await' operator cannot be used in an unsafe context
// Line: 12
// Compiler options: -langversion:future -unsafe

using System;
using System.Threading.Tasks;

unsafe class C
{
	public async Task Test ()
	{
		await Call ();
	}
	
	static Task Call ()
	{
		return null;
	}
}
