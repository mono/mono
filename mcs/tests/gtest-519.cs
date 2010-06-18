class G<T>
{
}

interface I
{
	void Foo<T> () where T : G<T>;
}

class A : I
{
	public void Foo<U> () where U : G<U>
	{
	}

	public static void Main ()
	{
	}
}