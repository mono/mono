// Compiler options: -langversion:future

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

class async
{
	async (async arg)
	{
	}
}

[async]
class asyncAttribute: Attribute
{
	delegate async async (async async);
}