namespace NUnit.Framework {

  using System;
  using System.Diagnostics;
  
  /// <summary>Thrown when an assertion failed. Here to preserve the inner
  /// exception and hence its stack trace.</summary>
  public class NUnitException: ApplicationException {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public NUnitException(string message, Exception inner) :
      base(message, inner) { }
    /// <summary>
    /// 
    /// </summary>
    public bool IsAssertionFailure {
      get { return InnerException.GetType() == typeof(AssertionFailedError); }
    }
  }
}

