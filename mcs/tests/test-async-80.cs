using System.Threading.Tasks;
using System;

class X
{
	public async Task Test<T, U> (int arg)
	{
		await Task.Run (async () => {
					await Task.Run (async () => {
						Console.WriteLine (this);
					});
				return arg;
			}
		);
	}

	public static void Main ()
	{
		new X().Test<int, long>(1).Wait ();
	}
}