using System;
using System.Threading.Tasks;

class MainClass
{
	public static void Main ()
	{
		new MainClass ().Foo ().Wait ();
	}

	private async Task<int> Foo() 
	{
		await Task.Delay(1);
		return 42;
	}

	private async Task Bar()
	{
		Console.WriteLine($"Something {await Foo()}");
	}
}
