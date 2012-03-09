// CS4034: The `await' operator can only be used when its containing lambda expression is marked with the `async' modifier
// Line: 11

using System;
using System.Threading.Tasks;

class C
{
	public void Test ()
	{
		Action a = () => await Task.FromResult (1);
	}
}
