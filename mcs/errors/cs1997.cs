// CS1997: `C.Test()': A return keyword must not be followed by an expression when async method returns `Task'. Consider using `Task<T>' return type
// Line: 12

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
