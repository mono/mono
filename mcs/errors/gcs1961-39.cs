// CS1961: The covariant type parameter `V' must be contravariantly valid on `I<V>.Foo<T>(T)'
// Line: 6

interface I<out V>
{
	void Foo<T> (T t) where T : V;
}
