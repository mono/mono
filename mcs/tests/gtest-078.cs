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
		S? nullable = null;
		using (var a = nullable) {
		}

		if (S.hit != 0)
			return 1;

		using (var s = new S ()) {
		}

		if (S.hit != 1)
			return 2;

		C c = null;
		GenMethod (c);
		
		using (S? a = nullable, b = nullable) {
		}
		
		if (S.hit != 1)
			return 3;

		Console.WriteLine ("ok");
		return 0;
	}

	static void GenMethod<T> (T t) where T : IDisposable
	{
		using (T t2 = t) {
		}
	}
}
