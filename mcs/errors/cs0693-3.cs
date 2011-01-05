// CS0693: Type parameter `T' has the same name as the type parameter from outer type `C<T>'
// Line: 7
// Compiler options: -warnaserror -warn:3

class C<T>
{
	void Foo<T> (T t)
	{
	}
}
