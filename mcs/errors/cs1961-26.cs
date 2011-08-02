// CS1961: The covariant type parameter `T' must be invariantly valid on `B<T>(A<T>)'
// Line: 8

interface A<T>
{
}

delegate void B<out T> (A<T> a);
