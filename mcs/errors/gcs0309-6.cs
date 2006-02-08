// CS0309: The type `B<int>' must be convertible to `D<B<int>>' in order to use it as parameter `X' in the generic type or method `C<X>'
// Line: 3
class A : C<B<int>> {}
class B<X> {}
interface C<X> where X : D<X> {}
interface D<X> {}
