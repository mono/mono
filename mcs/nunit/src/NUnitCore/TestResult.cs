namespace NUnit.Framework {

  using System;
  using System.Collections;
  using System.Threading;

  /// <summary>A <c>TestResult</c> collects the results of executing
  /// a test case. It is an instance of the Collecting Parameter pattern.
  /// </summary><remarks>
  /// The test framework distinguishes between failures and errors.
  /// A failure is anticipated and checked for with assertions. Errors are
  /// unanticipated problems like an <c>ArgumentOutOfRangeException</c>.
  /// </remarks><seealso cref="ITest"/>
  public class TestResult {
    private ArrayList fFailures;
    private ArrayList fErrors;
    private ArrayList fListeners;
    private int fRunTests;
    private bool fStop;
    /// <summary>
    /// 
    /// </summary>
    public TestResult() {
      fFailures= new ArrayList();
      fErrors= new ArrayList();
      fListeners= new ArrayList();
    }

    /// <summary>Adds an error to the list of errors. The passed in exception
    /// caused the error.</summary>
    public void AddError(ITest test, Exception t) {
      lock(this) {
        fErrors.Add(new TestFailure(test, t));
        foreach (ITestListener l in CloneListeners()) {
          l.AddError(test, t);
        }
      }
    }

    /// <summary>Adds a failure to the list of failures. The passed in
    /// exception caused the failure.</summary>
    public void AddFailure(ITest test, AssertionFailedError t) {
      lock(this)
      {
        fFailures.Add(new TestFailure(test, t));
        foreach (ITestListener l in CloneListeners()) {
          l.AddFailure(test, t);
        }
      }
    }

    /// <summary>Registers a TestListener.</summary>
    public void AddListener(ITestListener listener) {
      lock(this)
        fListeners.Add(listener);
    }

    /// <summary>Returns a copy of the listeners.</summary>
    private ArrayList CloneListeners() {
      lock(this)
        return (ArrayList)fListeners.Clone();
    }

    /// <summary>Informs the result that a test was completed.</summary>
    public void EndTest(ITest test) {
      foreach (ITestListener l in CloneListeners()) {
        l.EndTest(test);
      }
    }

    /// <value>Gets the number of detected errors.</value>
    public int ErrorCount {
      get {return fErrors.Count; }
    }  

    /// <value>Returns an IEnumerable for the errors.</value>
    public IEnumerable Errors {
      get { return fErrors; }
    }

    /// <value>Gets the number of detected failures.</value>
    public int FailureCount {
      get {return fFailures.Count; }
    }

    /// <value>Returns an IEnumerable for the failures.</value>
    public IEnumerable Failures {
      get { return fFailures; }
    }

    /// <summary>Runs a TestCase.</summary>
    protected class ProtectedProtect: IProtectable {
      private TestCase fTest;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="test"></param>
      public ProtectedProtect(TestCase test) {
        fTest = test;
      }
		/// <summary>
		/// 
		/// </summary>
      public void Protect() {
        fTest.RunBare();
      }
    }

    /// <summary>Unregisters a TestListener</summary>
    public void RemoveListener(ITestListener listener) {
      lock(this) {
        fListeners.Remove(listener);
      }
    }

	/// <summary>Runs a TestCase.</summary>
    internal void Run(TestCase test) {
      StartTest(test);
      IProtectable p = new ProtectedProtect(test);
      RunProtected(test, p);

      EndTest(test);
    }

    /// <value>Gets the number of run tests.</value>
    public int RunCount {
      get {return fRunTests; }
    }

    /// <summary>Runs a TestCase.</summary>
    public void RunProtected(ITest test, IProtectable p) {
      try {
        p.Protect();
      }
      catch (NUnitException e) {
        if (e.IsAssertionFailure)
          AddFailure(test, (AssertionFailedError)e.InnerException);
        else
          AddError(test, e.InnerException);
      }
      catch (ThreadAbortException e) { // don't catch by accident
        throw e;
      }
      catch (AssertionFailedError e) {
        AddFailure(test, e);
      }
      catch (System.Exception e) {
        AddError(test, e);
      }
    }

    /// <summary>Checks whether the test run should stop.</summary>
    public bool ShouldStop {
      get {return fStop; }
    }

    /// <summary>Informs the result that a test will be started.</summary>
    public void StartTest(ITest test) {
      int count = test.CountTestCases;
      lock(this)
        fRunTests += count;
      foreach (ITestListener l in CloneListeners()) {
        l.StartTest(test);
      }
    }

    /// <summary>Marks that the test run should stop.</summary>
    public void Stop() {
      fStop= true;
    }

    /// <value>Returns whether the entire test was successful or not.</value>
    public bool WasSuccessful {
      get {
        lock(this)
          return (FailureCount == 0) && (ErrorCount == 0);
      }
    }
  }
}
    
