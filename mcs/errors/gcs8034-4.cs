// CS8034: Contravariant type parameters can only be used as input arguments to a method
// Line: 11
// Compiler options: -langversion:future

interface A<T>
{
}

interface B<in T>
{
	A<A<T>> C();
}
