// cs0119: Expression denotes a 'method group', where a 'variable', 'value' or 'type' was expected
// Line: 9

public class App {

  public static void Main() {}

  SomeEnum SomeEnum() {
    return SomeEnum.First;
  }

}

enum SomeEnum { First, Second };
