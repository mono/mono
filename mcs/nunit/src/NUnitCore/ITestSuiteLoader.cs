namespace NUnit.Runner {

  using System;

  /// <summary>An interface to define how a test suite should be
  /// loaded.</summary>
  public interface ITestSuiteLoader {
    /// <summary>
    /// 
    /// </summary>
    Type Load(string suiteClassName);
  /// <summary>
  /// 
  /// </summary>
    Type Reload(Type aType);
  }
}
