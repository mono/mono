using System;
using System.Threading.Tasks;

public class CompilerBug
{
	public static void Main ()
	{
		var res = Foo ().Result;
		Console.WriteLine (res);
		return;
	}

	static async Task<string> Foo ()
	{
		Action fnAction;
		{
			fnAction = () => { };
		}
		await Task.Delay (10);
		{
			fnAction ();
		}
		return "val";
	}
}
