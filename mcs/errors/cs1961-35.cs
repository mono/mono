// CS1961: The covariant type parameter `T' must be contravariantly valid on `A<T>.B(T)'
// Line: 6

interface A<out T>
{
	void B(T t);
}
