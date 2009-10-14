// CS0023: The `++' operator cannot be applied to operand of type `bool'
// Line: 13

public class C{
  public static bool Foo{
    get{
      return false;
    }
    set{
    }	
  }
  public static void Main(){
    Foo++;
  }
}
