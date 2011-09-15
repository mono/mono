// CS4006: __arglist is not allowed in parameter list of async methods
// Line: 9
// Compiler options: -langversion:future

using System.Threading.Tasks;

class C
{
	public async Task Test (__arglist)
	{
		await Call ();
	}
	
	static Task Call ()
	{
		return null;
	}
}
