// cs0187.cs: No such operator '++' defined for type 'bool'
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
