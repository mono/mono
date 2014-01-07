using System.Threading.Tasks;
using System;

class X
{
	readonly Func<string, Task> action = null;

	public static void Main ()
	{
	}

	protected async Task TestAsync ()
	{
		await action ("");
	}
}