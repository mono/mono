// CS7094: The `await' operator cannot be used in the filter expression of a catch clause
// Line: 12

using System.Threading.Tasks;

class Test
{
	async static Task M1 ()
	{
		try {
		}
		catch if (await Task.Factory.StartNew (() => false)) {
		}
	}
}