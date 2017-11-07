// CS8178: `await' cannot be used in an expression containing a call to `X.Wrap(int)' because it returns by reference
// Line: 12

using System.Threading.Tasks;

class X
{
	int x;

	async Task Test ()
	{
		Foo (ref Wrap (await Task.FromResult (1))) = 4;
	}

	ref int Wrap (int arg)
	{
		return ref x;
	}

	static ref int Foo (ref int arg)
	{
		return ref arg;
	}
}