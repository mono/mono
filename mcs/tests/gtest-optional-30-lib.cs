// Compiler options: -t:library

public static class Lib
{
	public static T Foo<T> (T x = default (T))
	{
		return x;
	}
}