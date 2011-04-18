// CS1961: The contravariant type parameter `T' must be covariantly valid on `A<T>.B()'
// Line: 6

interface A<in T>
{
	T B();
}
