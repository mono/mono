// CS1961: The contravariant type parameter `T' must be invariantly valid on `B<T>(A<A<T>>)'
// Line: 9
// Compiler options: -langversion:future

interface A<T>
{
}

delegate void B<in T> (A<A<T>> a);
