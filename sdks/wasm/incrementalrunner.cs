using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;


using NUnit.Framework;
using NUnit.Framework.Internal;
using NUnit.Framework.Api;
using NUnit.Framework.Internal.Filters;
using NUnitLite.Runner;


public class MyListener : ITestListener
{
	int scount, count;
	public void TestStarted(ITest test)
	{
		if (test.IsSuite)
			++scount;
		else
			++count;
		if (!test.IsSuite)
			Console.WriteLine ("{0} {1}/{2}", test.FullName, scount, count);
	}

    public void TestFinished(ITestResult result)
	{
	}

	public void TestOutput(TestOutput testOutput)
	{
	}
}

class AbstractAction {
	internal TestExecutionContext context, initialCtx;
	internal TestResult testResult;
	internal IncrementalTestRunner runner;
	Test test;
	AbstractAction parent;

	internal AbstractAction (Test test, AbstractAction parent, IncrementalTestRunner runner)
	{
		this.test = test;
		this.runner = runner;
		this.parent = parent;
		this.initialCtx = parent.context;
		this.testResult = test.MakeTestResult ();
	}

	internal AbstractAction (Test test, TestExecutionContext initialCtx, IncrementalTestRunner runner)
	{
		this.test = test;
		this.runner = runner;
		this.initialCtx = initialCtx;
		this.testResult = test.MakeTestResult ();
	}

	protected void SetupContext () {
		this.context = new TestExecutionContext (this.initialCtx);
		this.context.CurrentTest = this.test;
		this.context.CurrentResult = this.testResult;
		((ITestListener)IncrementalTestRunner.listener.GetValue (this.context)).TestStarted (this.test);
		this.context.StartTime = DateTime.Now;

		IncrementalTestRunner.setCurrentContext.Invoke (null, new object[] { this.context });

		long startTicks = Stopwatch.GetTimestamp();
		runner.finallyDelegate.Set (context, startTicks, testResult);
	}

	protected void WorkItemComplete ()
	{
		if (this.parent != null)
			this.parent.testResult.AddResult (testResult);

		runner.finallyDelegate.Complete ();
		// _state = WorkItemState.Complete;
		// if (Completed != null)
		//    Completed(this, EventArgs.Empty);
	}
}

class TestAction : AbstractAction {
	TestMethod test;

	public TestAction (TestMethod test, AbstractAction parent, IncrementalTestRunner runner) : base (test, parent, runner)
	{
		this.test = test;
	}

	public void PerformWork () {
		SetupContext ();
        try
        {
			if (IncrementalTestRunner.testSkipCount > 0) {
				--IncrementalTestRunner.testSkipCount;
				testResult = ((Test)test).MakeTestResult ();
				testResult.SetResult (ResultState.Success);
			} else {
				testResult = test.MakeTestCommand ().Execute(this.context);
			}
        }
        finally
        {
            WorkItemComplete ();
        }
	}
}

class TestSuiteAction : AbstractAction {
	TestSuite testSuite;
	List<ITest> children;

	public TestSuiteAction (TestSuite test, AbstractAction parent, IncrementalTestRunner runner) : base (test, parent, runner)
	{
		this.testSuite = test;
	}

	public TestSuiteAction (TestSuite test, TestExecutionContext initialCtx, IncrementalTestRunner runner) : base (test, initialCtx, runner)
	{
		this.testSuite = test;
	}

	void EnqueueChildren () {
		foreach (var t in children) {
			if (t is TestSuite) {
				var a = new TestSuiteAction ((TestSuite)t, this, runner);
				runner.actions.Enqueue (a.PerformWork);
			} else {
				var a = new TestAction ((TestMethod)t, this, runner);
				runner.actions.Enqueue (a.PerformWork);
			}
		}
	}

	private void SkipChildren ()
	{
		foreach (var t in children) {
			TestResult result = ((Test)t).MakeTestResult ();
			if (testResult.ResultState.Status == TestStatus.Failed)
				result.SetResult (ResultState.Failure, "TestFixtureSetUp Failed");
			else
				result.SetResult (testResult.ResultState, testResult.Message);
			testResult.AddResult (result);
		}
	}

	void SkipFixture (ResultState resultState, string message, string stackTrace)
	{
		testResult.SetResult (resultState, message, stackTrace);
		SkipChildren ();
	}

	private string GetSkipReason()
	{
		return (string)testSuite.Properties.Get(PropertyNames.SkipReason);
	}

	private string GetProviderStackTrace()
	{
		return (string)testSuite.Properties.Get(PropertyNames.ProviderStackTrace);
	}

	public void PerformWork () {
		SetupContext ();

		children = new List <ITest> ();

		if (testSuite.HasChildren) {
			foreach (Test test in testSuite.Tests) {
				if (runner.filter.Pass(test))
					children.Add (test);
			}
		}

		switch (testSuite.RunState) {
		default:
		case RunState.Runnable:
		case RunState.Explicit:
			testResult.SetResult (ResultState.Success);
			PerformOneTimeSetUp ();

			if (children.Count > 0) {
			    switch (testResult.ResultState.Status)
			    {
				case TestStatus.Passed:
					EnqueueChildren ();
					break;

				case TestStatus.Skipped:
				case TestStatus.Inconclusive:
				case TestStatus.Failed:
					SkipChildren ();
					break;
				}
			}

			//all action enqueuing actions have already run, now it's ok to enqueue this.
			runner.actions.Enqueue (() => runner.actions.Enqueue (this.PerformOneTimeTearDown));
			break;

		case RunState.Skipped:
			SkipFixture (ResultState.Skipped, GetSkipReason(), null);
			break;

		case RunState.Ignored:
			SkipFixture (ResultState.Ignored, GetSkipReason(), null);
			break;

		case RunState.NotRunnable:
			SkipFixture (ResultState.NotRunnable, GetSkipReason(), GetProviderStackTrace());
			break;
		}

		//all action enqueuing actions have already run, now it's ok to enqueue this.   
	   runner.actions.Enqueue (() => runner.actions.Enqueue (this.WorkItemComplete));
   }

	private void PerformOneTimeTearDown ()
	{
		IncrementalTestRunner.setCurrentContext.Invoke (null, new object[] { this.context });
		testSuite.GetOneTimeTearDownCommand ().Execute(this.context);
	}

	private void PerformOneTimeSetUp ()
	{
		try
		{
			testSuite.GetOneTimeSetUpCommand().Execute(this.context);

			// SetUp may have changed some things
			this.context.UpdateContext();
		}
		catch (Exception ex)
		{
			if (ex is NUnitException || ex is System.Reflection.TargetInvocationException)
				ex = ex.InnerException;

			testResult.RecordException (ex);
		}
	}
}

public class IncrementalTestRunner {
	internal static PropertyInfo listener = typeof (TestExecutionContext).GetProperty ("Listener", BindingFlags.Instance | BindingFlags.NonPublic);
	internal static MethodInfo setCurrentContext = typeof (TestExecutionContext).GetMethod ("SetCurrentContext", BindingFlags.Static | BindingFlags.NonPublic);

	IDictionary loadOptions;
	internal ITestFilter filter = TestFilter.Empty;
	UnhandledExceptionEventHandler crashHandler;

	internal FinallyDelegate finallyDelegate;
	ITestAssemblyBuilder builder;
	// ITestAssemblyRunner runner;

	public IncrementalTestRunner () {
        this.finallyDelegate = new FinallyDelegate();
		this.builder = new NUnitLiteTestAssemblyBuilder();
        // this.runner = new NUnitLiteTestAssemblyRunner(new NUnitLiteTestAssemblyBuilder(), finallyDelegate);

		this.loadOptions = new Hashtable ();
	}


	TestSuite testSuite;

	public void Add (Assembly assembly) {
		testSuite = builder.Build (assembly, loadOptions);
		if (testSuite == null)
			throw new Exception ("Could not load " + assembly);
		// if (!runner.Load (assembly, loadOptions))
		// 	throw new Exception ("Could not load " + assembly);
	}

	public void Exclude (string categories) {
		var excludeFilter = new NotFilter (new SimpleCategoryExpression(categories).Filter);
		filter = And (filter, excludeFilter);
	}

	public void RunOnly (string testName) {
		var nameFilter = new SimpleNameFilter (testName);
		filter = And (filter, nameFilter);
	}

	internal Queue<Action> actions = new Queue<Action> ();
	int test_step_count;
	string test_status;
	internal static int testSkipCount;

	public string Status {
		get { return test_status; }
	}

	TestSuiteAction rootAction;
	void QueueActions (TestSuite suite) {
		TestExecutionContext context = new TestExecutionContext ();
		if (this.loadOptions.Contains ("WorkDirectory"))
			context.WorkDirectory = (string)this.loadOptions ["WorkDirectory"];
		else
			context.WorkDirectory = Environment.CurrentDirectory;

		listener.SetValue (context, new MyListener ());
		rootAction = new TestSuiteAction (suite, context, this);
		actions.Enqueue (rootAction.PerformWork);
	}

	public void SkipFirst (int tsc) {
		testSkipCount = tsc;
	}

	public void Start (int step) {
		if (actions.Count > 0)
			throw new Exception ("Test already started");

        crashHandler = new UnhandledExceptionEventHandler(TopLevelHandler);
        AppDomain.CurrentDomain.UnhandledException += crashHandler;

		this.test_step_count = step;
		QueueActions (testSuite);
	}

	public bool Step () {
		int remaining = test_step_count;

		while (actions.Count > 0 && remaining > 0) {
			var a = actions.Dequeue ();
			a ();
			--remaining;
		}

		if (actions.Count == 0) {
			var res = new ResultReporter (rootAction.testResult, Console.Out);
			if ((res.Summary.FailureCount + res.Summary.ErrorCount) > 0)
				test_status = "FAIL";
			else
				test_status = "PASS";

			res.ReportResults ();
			return false;
		}

		return true;
	}

	void TopLevelHandler(object sender, UnhandledExceptionEventArgs e)
	{
		// Make sure that the test harness knows this exception was thrown
		if (finallyDelegate != null)
			finallyDelegate.HandleUnhandledExc(e.ExceptionObject as Exception);
	}

	static ITestFilter And (ITestFilter filter, ITestFilter other) {
        if (filter.IsEmpty)
            filter = other;
        else if (filter is AndFilter)
            ((AndFilter)filter).Add(other);
        else
            filter = new AndFilter(filter, other);
		return filter;
	}
}