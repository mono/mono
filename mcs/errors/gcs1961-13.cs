// CS1961: The contravariant type parameter `T' must be covariantly valid on `B<T>.A'
// Line: 9
// Compiler options: -langversion:future

interface A<out T>
{
}

interface B<in T>
{
	A<T> A { get; }
}
