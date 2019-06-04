// CS8177: Async methods cannot use by-reference variables
// Line: 12

using System.Threading.Tasks;

class X
{
	int x;

	async Task Test ()
	{
		ref int y = ref x;
		await Task.Yield ();
	}
}