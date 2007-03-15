using System;

namespace test
{

  public class App
  {
    public static void Main() {
    
    }
  }

  public class ThisClass<T, O>
    where T: ThisClass<T, O>
    where O: OtherClass<O, T>
  {
    internal int dummy;
  }

  public class OtherClass<O, T>
    where O: OtherClass<O, T> 
    where T: ThisClass<T, O>
  {
    public void Test(T tc) {
      tc.dummy = 0;
    }
  }
}
