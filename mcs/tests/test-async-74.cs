using System;
using System.Threading.Tasks;

class AwaitGotoBug
{
	public async Task Test()
	{
		using ((IDisposable)null)
		{
			retry:

			if (Equals(1, 2))
			{
				await Task.Yield();
				goto retry;
			}
			else
			{
				await Task.Yield();
			}
		}
	}

	public static void Main ()
	{
	}
}