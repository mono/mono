// CS0126: An object of a type convertible to `string' is required for the return statement
// Line: 7

using System.Threading.Tasks;

class A
{
	static async Task<string> Test ()
	{
		await CallAsync ();
		return;
	}
	
	static Task<string> CallAsync ()
	{
		return null;
	}
}
