namespace NUnit.Framework {

  using System;
  using System.Diagnostics;
  using System.IO;
  using System.Text;
  
  /// <summary>A <c>TestFailure</c> collects a failed test together with
  /// the caught exception.</summary>
  /// <seealso cref="TestResult"/>
  public class TestFailure {
    private readonly ITest failedTest;
    private readonly Exception thrownException;

    /// <summary>Constructs a TestFailure with the given test and
    /// exception.</summary>
    public TestFailure(ITest theFailedTest, Exception theThrownException) {
      failedTest= theFailedTest;
      thrownException= theThrownException;
    }

    /// <value>Gets the failed test.</value>
    public ITest FailedTest { 
      get { return failedTest; }
    }

    /// <value>True if it's a failure, false if error.</value>
    public bool IsFailure {
      get { return thrownException is AssertionFailedError; }
    }

    /// <value>Gets the thrown exception.</value>
    public Exception ThrownException {
      get { return thrownException; }
    }

    /// <summary>Returns a short description of the failure.</summary>
    public override string ToString() {
      StringBuilder buffer= new StringBuilder();
      buffer.Append(failedTest+": "+thrownException.Message);
      return buffer.ToString();
    }
  }
}
