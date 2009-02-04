// CS8038: Covariant type parameters cannot be used as method parameters
// Line: 7
// Compiler options: -langversion:future

interface A<out T>
{
	void B(out T t);
}
