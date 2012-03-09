// CS4014: The statement is not awaited and execution of current method continues before the call is completed. Consider using `await' operator
// Line: 17
// Compiler options: -warnaserror

using System;
using System.Threading.Tasks;

class C
{
	static Task Method ()
	{
		return Task.FromResult (1);
	}
	
	static void TestAsync ()
	{
		Func<Task> a = async () => {
			await Method ();
			Method ();
		};
	}
}
