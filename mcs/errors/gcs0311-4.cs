// CS0311: The type `B<int>' cannot be used as type parameter `X' in the generic type or method `C<X>'. There is no implicit reference conversion from `B<int>' to `D<B<int>>'
// Line: 3
class A : C<B<int>> {}
class B<X> {}
interface C<X> where X : D<X> {}
interface D<X> {}
