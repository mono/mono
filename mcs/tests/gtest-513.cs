using System;

struct S : IDisposable
{
	public static int hit;

	void IDisposable.Dispose ()
	{
		hit++;
	}

	public void Dispose ()
	{
		throw new ApplicationException ();
	}
}

class C : IDisposable
{

	void IDisposable.Dispose ()
	{
	}

	public void Dispose ()
	{
		throw new ApplicationException ();
	}
}

class Test
{
	public static int Main ()
	{
		using (new S? ()) {
		}

		if (S.hit != 0)
			return 1;

		using (new C ()) {
		}

		var s = new S ();
		using (s) {
		}

		if (S.hit != 1)
			return 2;

		GenMethod (s);

		if (S.hit != 2)
			return 3;

		Console.WriteLine ("ok");
		return 0;
	}

	static void GenMethod<T> (T t) where T : IDisposable
	{
		using (t) {
		}
	}
}
