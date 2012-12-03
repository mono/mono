public interface SomeInterface
{
  bool Valid { get; }
}

public struct SomeStruct : SomeInterface
{
  public bool Valid {
    get {     
      return false;
    }
  }
}

public class Test
{
  public static void Fun<T>(T t) where T:SomeInterface {
    bool a = t.Valid;
  }

  public static void Main()
  {
    Fun(new SomeStruct());
  }
}
