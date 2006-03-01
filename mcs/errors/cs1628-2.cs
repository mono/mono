// cs1628-2.cs: Cannot use ref or out parameter `i' inside an anonymous method block
// Line: 8

public class Test {
  public void test(out int i) {
    i = 0;
    System.EventHandler test = delegate {
      i++;
    };
  }
}





