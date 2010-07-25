// CS1961: The contravariant type parameter `T' must be invariantly valid on `B<T>.A'
// Line: 9
// Compiler options: -langversion:future

interface A<T>
{
}

interface B<in T>
{
	A<A<T>> A { get; }
}
