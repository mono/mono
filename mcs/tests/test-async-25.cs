// Compiler options: -langversion:future
using System;
using System.Threading.Tasks;

class Program
{
	static void CompilationTestOnly ()
	{
		var t = new Task<int> (() => 5);
		var t2 = Task.Run (() => { return t; });
	}
	
	public static void Main ()
	{
	}
}
