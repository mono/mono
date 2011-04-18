// CS1574: XML comment on `Test' has cref attribute `Format()' that could not be resolved
// Line: 5
// Compiler options: -doc:dummy.xml -warnaserror
/// <summary>
/// <see cref="Format()" />
/// <see cref="Test()" />
/// </summary>
public class Test {
  static void Main () {
  }

  /// <summary />
  public Test (string a) {
  }

  /// <summary />
  public static void Format (object a)
  {
  }
}

