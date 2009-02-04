// CS8034: Contravariant type parameters can only be used as input arguments to a method
// Line: 9
// Compiler options: -langversion:future

interface A<T>
{
}

delegate void B<in T> (A<A<T>> a);
