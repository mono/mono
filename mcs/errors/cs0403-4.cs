// CS0403: Cannot convert null to the type parameter `T' because it could be a value type. Consider using `default (T)' instead
// Line: 8

class Test<T>
{
	public T Null ()
	{
		return (null);
	}
}
