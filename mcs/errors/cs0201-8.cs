// CS0201: Only assignment, call, increment, decrement, and new object expressions can be used as a statement
// Line: 11
// Compiler options: -langversion:future

using System;
using System.Threading.Tasks;

class C
{
	async Task<int> Test ()
	{
		Func<int> r = await Task.Factory.StartNew (() => () => 1);
	}
}
