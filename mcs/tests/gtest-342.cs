class Base<T> where T : Base<T>
{
	public static implicit operator T (Base<T> t)
	{
		return (T) t;
	}
}

class TestMain {
	public static void Main (string [] args)
	{
	}
}

