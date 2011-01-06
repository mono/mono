// CS0310: The type `Foo' must have a public parameterless constructor in order to use it as parameter `a' in the generic type or method `C<a>'
// Line: 14
class C <a> where a : new () {
}

class Foo {
  public Foo (int x) { }
}

class X
{
        static void Main ()
        {
          C<Foo> x;
        }
}
 

