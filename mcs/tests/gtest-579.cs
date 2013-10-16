public interface IA<T>
{
}


public class G<U, V> : IA<G<V, string>>
{
}

public class C
{
	static bool Test_2 <T2>(T2[] t)
	{
		return t is byte[];
	}

	public static int Main ()
	{
		G<long, short> p = new G<long, short> ();
		if (p is IA<G<string, string>>)
			return 1;

		if (Test_2 (new int [0]))
			return 2;

		if (!Test_2 (new byte [0]))
			return 3;

		return 0;
	}
}