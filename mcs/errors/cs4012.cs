// CS4012: Parameters or local variables of type `System.TypedReference' cannot be declared in async methods or iterators
// Line: 13

using System;
using System.Collections;
using System.Threading.Tasks;

class C
{
	public async void Test ()
	{
		int i = 2;
		TypedReference tr = __makeref (i);
		await Task.Factory.StartNew (() => 6);
	}
}
