// CS4010: Cannot convert async lambda expression to delegate type `System.Func<bool>'
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
