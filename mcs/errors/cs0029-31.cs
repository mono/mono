// CS0029: Cannot implicitly convert type `void' to `int'
// Line: 11

using System;
using System.Threading.Tasks;

class C
{
	public async Task<int> Test ()
	{
		return await Call ();
	}
	
	Task Call ()
	{
		return null;
	}
}
