// Compiler options: -doc:xml-049.xml -warnaserror 
/// <summary>
/// </summary>
public class Testje {
  static void Main () {
  }

  /// <summary>
  /// <see cref="Test" />
  /// <see cref="Format(object)" />
  /// </summary>
  private class A {
    /// <summary>
    /// <see cref="Test" />
    /// <see cref="Format(object)" />
    /// </summary>
    private class Test {
    }
  }

  /// <summary />
  public string Test {
    get { return ""; }
  }

  /// <summary />
  public static void Format (object a)
  {
  }
}

