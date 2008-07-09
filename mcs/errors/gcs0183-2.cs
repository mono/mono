// CS0183: The given expression is always of the provided (`T') type
// Line: 10
// Compiler options: -warnaserror -warn:1

class X
{
	static bool Foo<T> () where T : struct
	{
		T o = default (T);
		return o is T;
	}
}
