namespace NUnit.Extensions {
    
  using System;
  using System.Threading;
  
  using NUnit.Framework;
  
  /// <summary>A TestSuite for active Tests. It runs each
  /// test in a separate thread and until all
  /// threads have terminated.
  /// -- Aarhus Radisson Scandinavian Center 11th floor</summary>
  public class ActiveTestSuite: TestSuite {
    private int fActiveTestDeathCount;
	/// <summary>
	/// 
	/// </summary>
	/// <param name="result"></param>
    public override void Run(TestResult result) {
      fActiveTestDeathCount= 0;
      base.Run(result);
      WaitUntilFinished();
    }
    /// <summary>
    /// 
    /// </summary>
    public class ThreadLittleHelper {
      private ITest fTest;
      private TestResult fResult;
      private ActiveTestSuite fSuite;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="test"></param>
		/// <param name="result"></param>
		/// <param name="suite"></param>
      public ThreadLittleHelper(ITest test, TestResult result,
                                ActiveTestSuite suite) {
        fSuite = suite;
        fTest = test;
        fResult = result;
      }
	/// <summary>
	/// 
	/// </summary>
      public void Run() {
        try {
          fSuite.BaseRunTest(fTest, fResult);
        } finally {
          fSuite.RunFinished(fTest);
        }
      }
    }
  /// <summary>
  /// 
  /// </summary>
  /// <param name="test"></param>
  /// <param name="result"></param>
    public void BaseRunTest(ITest test, TestResult result) {
      base.RunTest(test, result);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="test"></param>
    /// <param name="result"></param>
    public override void RunTest(ITest test, TestResult result) {
      ThreadLittleHelper tlh = new ThreadLittleHelper(test, result, this);
      Thread t = new Thread(new ThreadStart(tlh.Run));
      t.Start();
    }
    void WaitUntilFinished() {
      lock(this) {
        while (fActiveTestDeathCount < TestCount) {
          try {
            Monitor.Wait(this);
          } catch (ThreadInterruptedException) {
            return; // TBD
          }
        } 
      }
    }
  /// <summary>
  /// 
  /// </summary>
  /// <param name="test"></param>
    public void RunFinished(ITest test) {
      lock(this) {
        fActiveTestDeathCount++;
        Monitor.PulseAll(this);
      }
    }
  }
}
