// CS4014: The statement is not awaited and execution of current method continues before the call is completed. Consider using `await' operator
// Line: 47
// Compiler options: -warnaserror

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

static class S
{
	public static A GetAwaiter (this X x)
	{
		return new A ();
	}
}

class X
{
	public X Foo ()
	{
		return this;
	}
}

class A : INotifyCompletion
{
	bool IsCompleted
	{
		get
		{
			return true;
		}
	}

	public void OnCompleted (Action a)
	{
	}

	int GetResult ()
	{
		return 3;
	}

	static async Task Test3 ()
	{
		X x = new X ();
		x.Foo ();
		await x.Foo ();
	}
}