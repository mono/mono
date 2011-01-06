// CS1961: The contravariant type parameter `T' must be covariantly valid on `A<T>.B()'
// Line: 7
// Compiler options: -langversion:future

interface A<in T>
{
	T B();
}
