using System;
using System.Threading.Tasks;

class Test
{
	public static async Task<int[]> Run ()
	{
		return new int[] {
			1, await Task.Factory.StartNew (() => 2)
		};
	}

	public static int Main ()
	{
		var t = Run ().Result;
		if (t [0] != 1)
			return 1;

		if (t [1] != 2)
			return 2;

		return 0;
	}
}