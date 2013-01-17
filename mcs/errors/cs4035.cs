// CS4035: The `await' operator can only be used when its containing anonymous method is marked with the `async' modifier
// Line: 11

using System;
using System.Threading.Tasks;

class C
{
	public void Test ()
	{
		Action a = delegate { await Task.FromResult (1); };
	}
}
