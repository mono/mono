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

	public A Instance
	{
		get
		{
			return new A ();
		}
	}

	async Task<int> Foo ()
	{
		int value = 1;

		{
			await Test (value,
				async () => {
					int b = value;
					await Instance.GetValue (Bar () + b);
				});
		}

		return 0;
	}

	int Bar ()
	{
		return 1;
	}

	T Test<T> (int arg, Func<T> func)
	{
		return func ();
	}
}

