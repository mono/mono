public interface IA
{
	int Foo (int x = 0);
}

public class A : IA
{
	public int Foo (int x)
	{
		return x;
	}

	private static int Bar<T> (T x) where T : A, IA
	{
		return x.Foo ();
	}

	public static int Main ()
	{
		if (Bar (new A ()) != 0)
			return 1;

		return 0;
	}
}