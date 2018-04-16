// Compiler options: -langversion:latest

static class X
{
	const int c1 = default;
	const int c2 = default (int);

	public static void Main ()
	{
		int a = default;
		var b = (int) default;
		const int c = default;
		var d = new[] { 1, default };
		dynamic e = default;
		int f = checked (default);
		(int a, int b) g = (1, default);
		var h = 1 != default;
		var i = default == M4 ();
	}

	static int M1 ()
	{
		return default;
	}

	static void M2 ()
	{
		try {
			throw new System.Exception ();
		} catch (System.Exception) when (default) {
		}

		if (default) {			
		}
	}

	static void M3 (int x = default)
	{
	}

	static System.Func<int> M4 ()
	{
		return () => default;
	}

	static void Foo (II a = default (II), II b = default, II c = (II) null)
	{
	}
}
/*
enum E
{
	A = default,
	B = default + 1
}
*/


interface II
{

}