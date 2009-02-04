// CS8033: Contravariant type parameters can only be used in input positions
// Line: 7
// Compiler options: -langversion:future

interface A<in T>
{
	T B();
}
