// CS1961: The contravariant type parameter `T' must be covariantly valid on `D<T>()'
// Line: 5
// Compiler options: -langversion:future

delegate T D<in T> ();
