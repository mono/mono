// CS1961: The covariant type parameter `U' must be contravariantly valid on `D<U>()'
// Line: 8

interface I<in T>
{
}

delegate I<U[]> D<out U> ();
