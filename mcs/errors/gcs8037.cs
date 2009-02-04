// CS8037: Contravariant type parameters cannot be used in output positions
// Line: 7
// Compiler options: -langversion:future

interface A<in T>
{
	void B(out T t);
}
