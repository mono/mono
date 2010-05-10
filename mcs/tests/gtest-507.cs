class A
{
	public static int Main ()
	{
		if (new B<object> () [4] != 2)
			return 1;
		
		return 0;
	}
}

public class B<T> {
	public int this[T key] {
		get { return 1; }
	}

	public int this[object key] {
		get { return 2; }
	}
}

