// CS8035: Covariant type parameters can only be used as return types or in interface inheritance
// Line: 11
// Compiler options: -langversion:future

interface A<T>
{
}

interface B<out T>
{
	A<A<T>> C();
}
