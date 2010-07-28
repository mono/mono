// CS0023: The `.' operator cannot be applied to operand of type `method group'
// Line: 9

public class App {

  public static void Main() {}

  SomeEnum SomeEnum() {
    return SomeEnum.First;
  }

}

enum SomeEnum { First, Second };
