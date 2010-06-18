class Top<X>
{
	public class C : I1<int>
	{
	}

	interface I1<T> : I2<T>
	{
	}

	interface I2<U>
	{
	}
}

class M
{
	public static int Main ()
	{
		return 0;
	}
}