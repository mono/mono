namespace NUnit.Framework {

  using System;
  
  /// <summary cref="System.Exception">Thrown when an assertion failed.</summary>
  public class AssertionFailedError: Exception {
	  /// <summary>
	  /// 
	  /// </summary>
	  /// <param name="message"></param>
    public AssertionFailedError (string message) : base(message) {}
  }
}
