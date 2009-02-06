// CS8034: Contravariant type parameters can only be used as type arguments in contravariant positions
// Line: 11
// Compiler options: -langversion:future

interface A<T>
{
}

interface B<in T>
{
	void C(A<A<T>> a);
}
