// cs0119-2.cs: `App.SomeEnum()' is a `method', which is not valid in the given context
// Line: 9

public class App {

  public static void Main() {}

  SomeEnum SomeEnum() {
    return SomeEnum.First;
  }

}

enum SomeEnum { First, Second };
