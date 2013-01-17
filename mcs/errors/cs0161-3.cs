// CS0161: `A.Test()': not all code paths return a value
// Line: 8

using System.Threading.Tasks;

class A
{
	static async Task<string> Test ()
	{
		await CallAsync ();
	}
	
	static Task<string> CallAsync ()
	{
		return null;
	}
}
