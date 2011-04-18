// CS1961: The contravariant type parameter `T' must be invariantly valid on `B<T>(A<T>)'
// Line: 8

interface A<T>
{
}

delegate void B<in T> (A<T> a);
