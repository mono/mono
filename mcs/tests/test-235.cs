//
// Compilation test: bug #47234
//
public class T {

	static void Foo (T t, T tt)
	{
	}

	static void Foo (params object[] theParams)
	{
	}

	public static int Main()
	{
		Foo (new T (), null);
                return 0;
	}
}






