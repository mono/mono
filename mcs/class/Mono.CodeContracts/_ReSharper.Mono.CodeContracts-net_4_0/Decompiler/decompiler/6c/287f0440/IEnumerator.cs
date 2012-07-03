// Type: System.Collections.Generic.IEnumerator`1
// Assembly: mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll

using System;
using System.Collections;

namespace System.Collections.Generic
{
  [__DynamicallyInvokable]
  public interface IEnumerator<out T> : IDisposable, IEnumerator
  {
    [__DynamicallyInvokable]
    T Current { [__DynamicallyInvokable] get; }
  }
}
