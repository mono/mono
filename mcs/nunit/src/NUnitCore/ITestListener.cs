namespace NUnit.Framework {

  using System;
  
  /// <summary>A Listener for test progress</summary>
  public interface ITestListener {
    /// <summary>An error occurred.</summary>
    void AddError(ITest test, Exception t);
    
    /// <summary>A failure occurred.</summary>
    void AddFailure(ITest test, AssertionFailedError t);
    
    /// <summary>A test ended.</summary>
    void EndTest(ITest test); 
    
    /// <summary>A test started.</summary>
    void StartTest(ITest test);
  }
  /// <summary>
  /// 
  /// </summary>
  public delegate void TestEventHandler(Object source, TestEventArgs e);
  /// <summary>
  /// 
  /// </summary>
  public class TestEventArgs : EventArgs{
  /// <summary>
  /// 
  /// </summary>
    protected ITest fTest;
  /// <summary>
  /// 
  /// </summary>
    protected TestEventArgs (){}
  /// <summary>
  /// 
  /// </summary>
  /// <param name="test"></param>
    public TestEventArgs (ITest test){
      fTest = test;
    }
    /// <summary>
    /// 
    /// </summary>
    public ITest Test{
      get{return fTest;}
    }
  }
  /// <summary>
  /// 
  /// </summary>
  public delegate void TestExceptionEventHandler(Object source,
                                                 TestExceptionEventArgs e);
  /// <summary>
  /// 
  /// </summary>
  public class TestExceptionEventArgs : TestEventArgs{
    private TestExceptionEventArgs(){}
    
    private Exception fThrownException;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="test"></param>
    /// <param name="e"></param>
    public TestExceptionEventArgs(ITest test, Exception e){
      //this(test);
      fTest = test;
      fThrownException = e;
    }
    /// <summary>
    /// 
    /// </summary>
    public Exception ThrownException{
      get{return fThrownException;}
    }
  }
}
