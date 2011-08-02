// CS1961: The covariant type parameter `T' must be invariantly valid on `A<T>.B(out T)'
// Line: 6

interface A<out T>
{
	void B(out T t);
}
