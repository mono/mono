using System;
using System.Threading.Tasks;

struct S
{
	public int value;
	public string str;
}

public class Program
{
	async Task<S> Foo ()
	{
		return new S {
			value = 1,
			str = await DoAsync ()
		};

	}

	static async Task<string> DoAsync ()
	{
		await Task.Yield ();
		return "asdafs";
	}

	static int Main ()
	{
		var res = new Program ().Foo ().Result;
		if (res.value != 1)
			return 1;

		return 0;
	}
}