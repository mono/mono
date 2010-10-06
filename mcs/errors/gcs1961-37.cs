// CS1961: The covariant type parameter `T' must be invariantly valid on `A<T>.B(ref T)'
// Line: 5
// Compiler options: -langversion:future

interface A<out T>
{
	void B(ref T t);
}
