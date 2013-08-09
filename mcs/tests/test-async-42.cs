using System;
using System.Threading.Tasks;

class A
{
	public Task<int> OpenAsync ()
	{
		return Task.FromResult (0);
	}
}

class C
{
	public static int Main ()
	{
		var c = new C ();
		return c.Foo ().Result;
	}

	public A Connection
	{
		get
		{
			return new A ();
		}
	}

	async Task<int> Foo ()
	{
		{
			await Test (
				async () => {
					await Connection.OpenAsync ();
				});
		}

		return 0;
	}

	T Test<T> (Func<T> func)
	{
		return func ();
	}
}

