// Type: System.Collections.Generic.IEnumerable`1
// Assembly: mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll

using System.Collections;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
  [TypeDependency("System.SZArrayHelper")]
  [__DynamicallyInvokable]
  public interface IEnumerable<out T> : IEnumerable
  {
    [__DynamicallyInvokable]
    IEnumerator<T> GetEnumerator();
  }
}
