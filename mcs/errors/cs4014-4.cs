// CS4014: The statement is not awaited and execution of current method continues before the call is completed. Consider using `await' operator
// Line: 12
// Compiler options: -warnaserror

using System;
using System.Threading.Tasks;

class C
{
	static async Task<int> TestAsync ()
	{
		new Task (() => {});
		return await Task.FromResult (2);
	}
}
