// CS0208: Cannot take the address of, get the size of, or declare a pointer to a managed type `T'
// Line: 8

class X
{
	public static void Foo<T> (T t)
	{
		object o = sizeof (T);
	}
}
