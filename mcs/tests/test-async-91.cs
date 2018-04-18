using System;
using System.Threading.Tasks;

class C : IDisposable
{
	public void Dispose ()
	{
		Console.WriteLine ("Disposed");
		TestClass.Passed++;
	}
}

public class TestClass
{
	public static int Passed;

	public static async Task Test ()
	{
		using (var device_resource = new C ()) {
			try {
				Console.WriteLine ("aa");
				return;
			} finally {
				await Task.Delay (0);
			}
		}
	}

	public static int Main()
	{
		Test ().Wait ();
		if (Passed != 1)
			return 1;

		Console.WriteLine ("PASSED");
		return 0;
	}
}