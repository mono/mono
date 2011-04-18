// CS1961: The covariant type parameter `U' must be contravariantly valid on `D<U>()'
// Line: 6

interface IContravariant<in T> { }

delegate IContravariant<U[]> D<out U> ();
