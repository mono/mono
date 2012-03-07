// CS4015: `C.SynchronousCall(int)': Async methods cannot use `MethodImplOptions.Synchronized'
// Line: 9

using System.Threading.Tasks;
using System.Runtime.CompilerServices;

class C
{
	[MethodImplAttribute(MethodImplOptions.Synchronized)]
	public static async Task SynchronousCall (int arg)
	{
		await Task.FromResult (1);
	}
}
