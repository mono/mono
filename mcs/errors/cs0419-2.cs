// CS0419: Ambiguous reference in cref attribute `A.this'. Assuming `Test.A.this[int]' but other overloads including `Test.A.this[string]' have also matched
// Line: 7
// Compiler options: -doc:dummy.xml -warnaserror
using System.Collections;

/// <summary>
///   <para><see cref="IDictionary.this[object]" /></para>
///   <para><see cref="A.this" /></para>
///   <para><see cref="B.this" /></para>
/// </summary>
public class Test
{
  static void Main()
  {
  }

  private class A
  {
    public object this[int index] {
      get { return null; }
    }

    public object this[string index] {
      get { return null; }
    }
  }

  private class B
  {
    public object this[int index] {
      get { return null; }
    }
  }
}

