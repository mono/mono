// CS1998: Async block lacks `await' operator and will run synchronously
// Line: 10
// Compiler options: -langversion:future -warnaserror

using System;
using System.Threading.Tasks;

class C
{
	static async Task<int> Method ()
	{
		return 0;
	}
	
	public static void Main ()
	{
	}
}
