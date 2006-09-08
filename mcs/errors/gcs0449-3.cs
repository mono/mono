// CS0449: The `class' or `struct' constraint must be the first constraint specified
// Line: 6

public class C<T>
{
      public void Foo<T>() where T : class, struct 
      {
      }
}