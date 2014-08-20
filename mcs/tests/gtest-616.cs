using System;

struct S : IDisposable
{
	public void Dispose ()
	{
	}
}

class A<T> where T : IDisposable
{
	public virtual bool Test<U> (U u) where U : T
	{
		using (u) {
			return false;
		}
	}
}

class B : A<S>
{
	public override bool Test<U> (U u)
	{
		using (u) {
			return true;
		}
	}

	public static int Main ()
	{
		var b = new B ();
		if (!b.Test (new S ()))
			return 1;

		return 0;
	}
}