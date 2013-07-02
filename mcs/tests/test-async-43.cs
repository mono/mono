using System;
using System.Threading.Tasks;

class A
{
	public Task<int> GetValue (int b)
	{
		return Task.FromResult (b);
	}
}

class C
{
	public static int Main ()
	{
		var c = new C ();
		return c.Foo ().Result;
	}

	public A Instance {
		get {
			return new A ();
		}
	}

	async Task<int> Foo ()
	{
		int value = 1;

		{
			int b = 3;
			await Test (value,
				async () => {
					await Instance.GetValue (b);
				});
		}
		
		return 0;
	}

	T Test<T> (int arg, Func<T> func)
	{
		return func ();
	}
}

