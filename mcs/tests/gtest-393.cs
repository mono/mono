class B<T>
{
}

[A(typeof (B<>))]
public class A : System.Attribute
{
	static int ret = 1;

	public A (System.Type type)
	{
		if (type == typeof (B<>))
			ret = 0;
	}

	public static int Main ()
	{
		typeof (A).GetCustomAttributes (true);
		return ret;
	}
}