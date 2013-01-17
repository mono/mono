using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class C
{
	async Task<int> M(int v)
	{
		{
			await Task.Yield();
			int vv = 0;
			Func<int> a = () => vv;
			vv = 3;
			Func<int> a2 = () => v + vv;
			if (a() != vv)
				return 1;

			if (a2() != 58)
				return 2;
		}
		return 0;
	}

	async Task<int> M2(int v, int o)
	{
		await Task.Yield();
		var xo = await Task.FromResult(v);
		int vv = v;
		Action a2;
		int b = o;
		{
			a2 = () => {
				v = 500;
				b = 2;
			};
		}

		await Task.Yield();
		a2 ();
		if (v != 500)
			return 1;

		return 0;
	}
	
	public static int Main()
	{
		var c = new C();
		if (c.M(55).Result != 0)
			return 1;

		if (c.M2(55, 22).Result != 0)
			return 2;

		return 0;
	}
}
