// CS1961: The covariant type parameter `V' must be contravariantly valid on `Foo<T>'
// Line: 9
// Compiler options: -langversion:future

interface I<out V>
{
	void Foo<T> (T t) where T : V;
}
