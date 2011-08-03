// CS1961: The contravariant type parameter `T' must be invariantly valid on `B<T>.A'
// Line: 8

interface A<T>
{
}

interface B<in T>
{
	A<A<T>> A { get; }
}
