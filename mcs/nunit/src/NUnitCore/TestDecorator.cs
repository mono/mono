namespace NUnit.Extensions {

  using System;

  using NUnit.Framework;

  /// <summary>A Decorator for Tests.</summary>
  /// <remarks>Use TestDecorator as the base class
  /// for defining new test decorators. TestDecorator subclasses
  /// can be introduced to add behaviour before or after a test
  /// is run.</remarks>
  public class TestDecorator: Assertion, ITest {
  /// <summary>
  /// 
  /// </summary>
    protected readonly ITest fTest;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="test"></param>
    public TestDecorator(ITest test) {
      fTest= test;
    }
    
    /// <summary>The basic run behaviour.</summary>
    public void BasicRun(TestResult result) {
      fTest.Run(result);
    }
	  /// <summary>
	  /// 
	  /// </summary>
    public virtual int CountTestCases {
      get { return fTest.CountTestCases; }
    }
  /// <summary>
  /// 
  /// </summary>
    public ITest GetTest {
      get { return fTest; }
    }
	  /// <summary>
	  /// 
	  /// </summary>
	  /// <param name="result"></param>
    public virtual void Run(TestResult result) {
      BasicRun(result);
    }
	  /// <summary>
	  /// 
	  /// </summary>
	  /// <returns></returns>
    public override string ToString() {
      return fTest.ToString();
    }
  }
}
