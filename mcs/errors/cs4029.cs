// CS4029: Cannot return an expression of type `void'
// Line: 15

using System;
using System.Threading.Tasks;

class C
{
	static void Foo<T> (Func<Task<T>> f)
	{
	}

	static void Main ()
	{
		Foo (async () => {
			return await Task.Factory.StartNew (() => { });
		});
	}
}