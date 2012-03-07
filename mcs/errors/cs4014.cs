// CS4014: The statement is not awaited and execution of current method continues before the call is completed. Consider using `await' operator
// Line: 18
// Compiler options: -warnaserror

using System;
using System.Threading.Tasks;

class C
{
	static Task Method ()
	{
		return Task.FromResult (1);
	}
	
	static async Task<int> TestAsync ()
	{
		Method ();
		return await Task.FromResult (2);
	}
}
