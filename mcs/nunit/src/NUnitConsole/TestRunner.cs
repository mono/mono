namespace NUnit.TextUI {

  using System;
  using System.IO;
  using System.Reflection;

  using NUnit.Framework;
  using NUnit.Runner;

  /// <summary>A command line based tool to run tests.</summary><remarks>
  /// <code>
  /// C:\NUnitConsole.exe /t [/wait] TestCaseClass
  /// </code>
  /// TestRunner expects the name of a TestCase class as argument.
  /// If this class defines a static <c>Suite</c> property it 
  /// will be invoked and the returned test is run. Otherwise all 
  /// the methods starting with "Test" having no arguments are run.
  ///
  /// When the wait command line argument is given TestRunner
  /// waits until the users types RETURN.
  ///
  /// TestRunner prints a trace as the tests are executed followed by a
  /// summary at the end.</remarks>
  public class TestRunner: BaseTestRunner {
    int fColumn = 0;
    TextWriter fWriter = Console.Out;

    /// <summary>
    /// Constructs a TestRunner.
    /// </summary>
    public TestRunner() {
    }

    /// <summary>
    /// Constructs a TestRunner using the given stream for all the output
    /// </summary>
    public TestRunner(TextWriter writer) : this() {
      if (writer == null)
        throw new ArgumentException("Writer can't be null");
      fWriter= writer;
    }
/// <summary>
/// 
/// </summary>
/// <param name="test"></param>
/// <param name="t"></param>
    public override void AddError(ITest test, Exception t) {
      lock(this)
        Writer.Write("E");
    }
/// <summary>
/// 
/// </summary>
/// <param name="test"></param>
/// <param name="t"></param>
    public override void AddFailure(ITest test, AssertionFailedError t) {
      lock (this)
        Writer.Write("F");
    }

    /// <summary>Creates the TestResult to be used for the test run.</summary>
    protected TestResult CreateTestResult() {
      return new TestResult();
    }
/// <summary>
/// 
/// </summary>
/// <param name="suite"></param>
/// <param name="wait"></param>
/// <returns></returns>
    protected TestResult DoRun(ITest suite, bool wait) {
      TestResult result= CreateTestResult();
      result.AddListener(this);
      long startTime= System.DateTime.Now.Ticks;
      suite.Run(result);
      long endTime= System.DateTime.Now.Ticks;
      long runTime= (endTime-startTime) / 10000;
      Writer.WriteLine();
      Writer.WriteLine("Time: "+ElapsedTimeAsString(runTime));
      Print(result);

      Writer.WriteLine();
            
      if (wait) {
        Writer.WriteLine("<RETURN> to continue");
        try {
          Console.ReadLine();
        }
        catch(Exception) {
        }
      }
      return result;    
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="test"></param>
    
    public override void EndTest(ITest test) {
    }
/// <summary>
/// 
/// </summary>
/// <returns></returns>
    public override ITestSuiteLoader GetLoader() {
      return new StandardTestSuiteLoader();
    }
/// <summary>
/// 
/// </summary>
/// <param name="result"></param>
    public void Print(TestResult result) {
      lock(this) {
        PrintErrors(result);
        PrintFailures(result);
        PrintHeader(result);
      }
    }

    /// <summary>Prints the errors to the standard output.</summary>
    public void PrintErrors(TestResult result) {
      if (result.ErrorCount != 0) {
        if (result.ErrorCount == 1)
          Writer.WriteLine("There was "+result.ErrorCount+" error:");
        else
          Writer.WriteLine("There were "+result.ErrorCount+" errors:");
                
        int i= 1;
        foreach (TestFailure failure in result.Errors) {
          Writer.WriteLine(i++ + ") "+failure+"("+failure.ThrownException.GetType().ToString()+")");
          Writer.Write(GetFilteredTrace(failure.ThrownException));
        }
      }
    }

    /// <summary>Prints failures to the standard output.</summary>
    public void PrintFailures(TestResult result) {
      if (result.FailureCount != 0) {
        if (result.FailureCount == 1)
          Writer.WriteLine("There was " + result.FailureCount + " failure:");
        else
          Writer.WriteLine("There were " + result.FailureCount + " failures:");
        int i = 1;
        foreach (TestFailure failure in result.Failures) {
          Writer.Write(i++ + ") " + failure.FailedTest);
          Exception t= failure.ThrownException;
          if (t.Message != "")
            Writer.WriteLine(" \"" + Truncate(t.Message) + "\"");
          else {
            Writer.WriteLine();
            Writer.Write(GetFilteredTrace(failure.ThrownException));
          }
        }
      }
    }

    /// <summary>Prints the header of the report.</summary>
    public void PrintHeader(TestResult result) {
      if (result.WasSuccessful) {
        Writer.WriteLine();
        Writer.Write("OK");
        Writer.WriteLine (" (" + result.RunCount + " tests)");
                
      } else {
        Writer.WriteLine();
        Writer.WriteLine("FAILURES!!!");
        Writer.WriteLine("Tests Run: "+result.RunCount+ 
                           ", Failures: "+result.FailureCount+
                           ", Errors: "+result.ErrorCount);
      }
    }

    /// <summary>Runs a Suite extracted from a TestCase subclass.</summary>
    static public void Run(Type testClass) {
      Run(new TestSuite(testClass));
    }
/// <summary>
/// 
/// </summary>
/// <param name="suite"></param>
    static public void Run(ITest suite) {
      TestRunner aTestRunner= new TestRunner();
      aTestRunner.DoRun(suite, false);
    }

    /// <summary>Runs a single test and waits until the user
    /// types RETURN.</summary>
    static public void RunAndWait(ITest suite) {
      TestRunner aTestRunner= new TestRunner();
      aTestRunner.DoRun(suite, true);
    }
/// <summary>
/// 
/// </summary>
/// <param name="message"></param>
    protected override void RunFailed(string message) {
      Console.Error.WriteLine(message);
      Environment.ExitCode = 1;
      throw new ApplicationException(message);
    }

    /// <summary>Starts a test run. Analyzes the command line arguments
    /// and runs the given test suite.</summary>
    public TestResult Start(string[] args) {
      bool wait = false;
      string testCase = ProcessArguments(args, ref wait);
      if (testCase.Equals(""))
        throw new ApplicationException("Usage: NUnitConsole.exe [/wait] testCaseName, where\n"
           + "name is the name of the TestCase class");

      try {
        ITest suite = GetTest(testCase);
        return DoRun(suite, wait);
      } catch (Exception e) {
        throw new ApplicationException("Could not create and run test suite.", e);
      }
    }
/// <summary>
/// 
/// </summary>
/// <param name="test"></param>
    public override void StartTest(ITest test) {
      lock (this) {
        Writer.Write(".");
        if (fColumn++ >= 40) {
          Writer.WriteLine();
          fColumn = 0;
        }
      }
    }
/// <summary>
/// 
/// </summary>
    protected TextWriter Writer {
      get { return fWriter; }
    }
  }
}
