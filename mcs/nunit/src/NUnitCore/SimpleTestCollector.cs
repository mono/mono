namespace NUnit.Runner {

  using System;

  /// <summary>
  /// An implementation of a TestCollector that considers
  /// a class to be a test class when it contains the
  /// pattern "Test" in its name
  /// <see cref="ITestCollector"/>
  /// </summary>
  public class SimpleTestCollector: ClassPathTestCollector {
	/// <summary>
	/// 
	/// </summary>
    public SimpleTestCollector() {
    }
	/// <summary>
	/// 
	/// </summary>
	/// <param name="classFileName"></param>
	/// <returns></returns>
    protected override bool IsTestClass(string classFileName) {
      return 
        (classFileName.EndsWith(".dll") || classFileName.EndsWith(".exe")) && 
        classFileName.IndexOf("Test") > 0;
    }
  }
}