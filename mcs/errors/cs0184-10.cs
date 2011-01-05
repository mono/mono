// CS0184: The given expression is never of the provided (`int') type
// Line: 10
// Compiler options: -warnaserror -warn:1

class X
{
	public void Foo<T> () where T : class
	{
		T t = default (T);
		if (t is int) {
		}
	}
}
