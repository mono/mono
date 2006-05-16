public class Foo<T> where T:Foo<T>
{
  public T n;

  public T next()
  {
    return n;
  }
}
 
public class Goo : Foo<Goo>
{
  public int x;
}
 
public class Test
{
  public static void Main()
  {
    Goo x = new Goo();
    
    x=x.next();
  }
}
