// CS1984: The `await' operator cannot be used in the body of a finally clause
// Line: 13
// Compiler options: -langversion:future

using System;
using System.Threading.Tasks;

class C
{
	public async Task Test ()
	{
		try {
		} finally {
			await Call ();
		}
	}
	
	static Task Call ()
	{
		return null;
	}
}
