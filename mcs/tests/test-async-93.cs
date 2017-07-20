using System;
using System.Threading.Tasks;
 
public class Test
{
	public static int Main()
	{
		var t = new Test ();
		t.Entry().Wait();
		if (t.caughtCounter != 1)
			return 1;

		return 0;
	}
 
	int caughtCounter;

	async Task Entry()
	{
		for (int i = 0; i < 5; ++i) {
			try {
				var result = Func(i);
				Console.WriteLine($"{i} result {result}");
			} catch (Exception e) {
				await Nothing();
				Console.WriteLine($"{i} caught");
				++caughtCounter;
			}
		}
	}
 
	bool Func(int i)
	{
		if (i == 0) {
			throw new Exception();
		} else {
			return true;
		}
	}
 
	async Task Nothing()
	{
	}
}