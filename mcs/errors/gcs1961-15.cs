// CS1961: The covariant type parameter `U' must be contravariantly valid on `D<U>()'
// Line: 9
// Compiler options: -langversion:future

interface I<in T>
{
}

delegate I<U[]> D<out U> ();
