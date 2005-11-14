// Compiler options: -doc:xml-044.xml -warnaserror -warn:4
using System.Xml;

/// <summary />
public class EntryPoint
{
  static void Main()
  {
  }

  /// <summary>
  /// <see cref="M:EntryPoint.B.Decide(System.Int32)" />
  /// </summary>
  internal class A
  {
    public virtual void Decide(int a)
    {
    }
  }

  internal class B : A
  {
  }
}

