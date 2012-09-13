using System;
using System.Threading.Tasks;

// contextual async during parsing

class A
{
	async Task<int> async ()
	{
		int async;
		throw new NotImplementedException ();
	}

	async Task async (int async)
	{
		throw new NotImplementedException ();
	}
	
	async void CastTest ()
	{
		var res = (int) await async ();
		var res2 = (Int32) await async ();
	}

	public static int Main ()
	{
		return 0;
	}
}

class B
{
	class async
	{
		async (async arg)
		{
		}
	}
}

class C
{
	static void Test (bool async)
	{
		var a = async ? Prop : 2;
	}

	static int Prop {
		get {
			return 3;
		}
	}
}

class async
{
	async (async arg)
	{
		int await = 0;
	}
}

class await
{
	await (await arg)
	{
	}
}

[async]
class asyncAttribute: Attribute
{
	delegate async async (async async);
}