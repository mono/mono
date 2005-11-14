// Compiler options: -doc:xml-043.xml -warnaserror -warn:4
/// <summary />
public class EntryPoint
{
  static void Main()
  {
  }

  private class A
  {
    public virtual void Decide(int a)
    {
    }
  }

  /// <summary>
  /// <see cref="Decide (int)" />
  /// </summary>
  private class B : A
  {
    public override void Decide(int a)
    {
    }
  }
}

