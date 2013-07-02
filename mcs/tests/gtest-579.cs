public interface IA<T>
{
}


public class G<U, V> : IA<G<V, string>>
{
}

public class C
{
	public static int Main ()
	{
		G<long, short> p = new G<long, short> ();
		if (p is IA<G<string, string>>)
			return 1;

		return 0;
	}
}