// CS0815: An implicitly typed local variable declaration cannot be initialized with `anonymous method'
// Line: 11

using System;
using System.Threading.Tasks;

class X
{
	public static void Main ()
	{
		Task.Run(async () => { var a = async () => { }; Console.WriteLine(a); });
	}
}