// CS1997: `C.Test()': A return keyword must not be followed by an expression when method is async
// Line: 12
// Compiler options: -langversion:future

using System;
using System.Threading.Tasks;

class C
{
	public async Task Test ()
	{
		await Call ();
		return null;
	}
	
	static Task Call ()
	{
		return null;
	}
}
