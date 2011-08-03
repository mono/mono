// CS1983: The return type of an async method must be void, Task, or Task<T>
// Line: 16
// Compiler options: -langversion:future

using System;
using System.Threading.Tasks;

class C
{
	static Task<int> GetInt ()
	{
		return null;
	}
	
	public static void Main ()
	{
		Func<bool> a = async () => { await GetInt (); };
	}
}
