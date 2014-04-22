// CS1654: Cannot assign to members of `f' because it is a `using variable'
// Line: 22

using System;

struct Foo : IDisposable
{
	public int this[int arg] {
		set { }
	}

	public void Dispose ()
	{
	}
}

class Bar
{
	static void Main ()
	{
		using (var f = new Foo ()) {
			f[0] = 1;
		}
	}
}