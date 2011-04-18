// CS1961: The contravariant type parameter `T' must be invariantly valid on `A<T>.B(out T)'
// Line: 5

interface A<in T>
{
	void B(out T t);
}
