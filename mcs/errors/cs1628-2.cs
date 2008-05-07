// CS1628: Parameter `i' cannot be used inside `anonymous method' when using `ref' or `out' modifier
// Line: 8

public class Test {
  public void test(out int i) {
    i = 0;
    System.EventHandler test = delegate {
      i++;
    };
  }
}





