// CS8035: Covariant type parameters can only be used as type arguments in covariant positions
// Line: 11
// Compiler options: -langversion:future

interface A<T>
{
}

interface B<out T>
{
	A<A<T>> A { get; }
}
