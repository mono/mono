using System;
using System.Threading.Tasks;

// contextual async, parser tests

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

namespace AsyncNS
{
	class Classes
	{
		class async
		{
		}
		
		void M ()
		{
			async local;
		}
	}

	namespace Namespaces
	{
		namespace async { }
	}
}

namespace AwaitNS
{
	class Formals
	{
		delegate void D (int x);
		static void M1 ()
		{
			D d1 = await => { };
			D d2 = (int await) => { };
			D d3 = delegate (int await) { };
		}
	}

	class Methods
	{
		void await () { }
		void M (Methods m)
		{
			m.await ();
			this.await ();
			// FIXME: await ();
		}
	}

	class Classes
	{
		class await { }
		void M ()
		{
			// FIXME: @await local = new @await ();
		}
	}

	class AnonTypes
	{
		static void M ()
		{
			var x = new { await = 1 };
			var y = x.await;
			int await = 2;
			var x2 = new { await };
		}
	}

	class Initializer
	{
		int await;

		static void M ()
		{
			var a = new Initializer () {
				await = 2
			};
		}
	}
}
