// CS8178: `await' cannot be used in an expression containing a call to `X.this[int]' because it returns by reference
// Line: 12

using System.Threading.Tasks;

class X
{
	int x;

	async Task Test ()
	{
		Foo (ref this [await Task.FromResult (1)]);
	}

	ref int this [int arg] => ref x;

	static void Foo (ref int arg)
	{
	}
}