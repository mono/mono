using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using NUnit.Framework;
using NUnit.Framework.Internal;
using NUnit.Framework.Api;
using NUnit.Framework.Internal.Filters;
using NUnitLite.Runner;


public class MyListener : ITestListener
{
	public void TestStarted(ITest test)
	{
		if (!test.IsSuite)
			Console.Write (".");
	}
		
    public void TestFinished(ITestResult result)
	{
	}

	public void TestOutput(TestOutput testOutput)
	{
	}
}

[Serializable]
public class CountingFilter: ITestFilter {
	int current, target, step;

	public CountingFilter (int step) {
		this.step = step;
	}

    public bool IsEmpty { get { return false; } }

	public bool Pass(ITest test) {
		//Skip any aggregates
		if (test.IsSuite)
			return true;
		bool res = current >= target && current < (target + step);
		++current;
		return res;
	}

	public void Bump () {
		target += step;
		current = 0;
	}
}

public class ResultCollector
{
	private int testCount;
	private int passCount;
	private int errorCount;
	private int failureCount;
	private int notRunCount;
	private int inconclusiveCount;
	private int ignoreCount;
	private int skipCount;
	private int invalidCount;


	public ResultCollector ()
	{
	}

	public bool Update (ITestResult result)
	{
		int tc = testCount;
		UpdateInternal (result);
		return tc < testCount;
	}

	List<ITestResult> badTests = new List<ITestResult> ();
	List<ITestResult> skippedTests = new List<ITestResult> ();
	List<ITestResult> inconclusiveTests = new List<ITestResult> ();

	private void UpdateInternal (ITestResult result)
	{
		if (result.Test.IsSuite) {
			foreach (ITestResult r in result.Children)
				UpdateInternal (r);
		}
		else {
			testCount++;

			switch (result.ResultState.Status)
			{
			case TestStatus.Passed:
				passCount++;
				break;
			case TestStatus.Skipped:
				if (result.ResultState == ResultState.Ignored)
					ignoreCount++;
				else if (result.ResultState == ResultState.Skipped) {
					skipCount++;
					skippedTests.Add (result);
				} else if (result.ResultState == ResultState.NotRunnable)
					invalidCount++;
				notRunCount++;
				break;
			case TestStatus.Failed:
				if (result.ResultState == ResultState.Failure)
					failureCount++;
				else
					errorCount++;
				badTests.Add (result);
				break;
			case TestStatus.Inconclusive:
				inconclusiveCount++;
				inconclusiveTests.Add (result);
				break;
			}
		}
	}

	public void Dump () {
		Console.WriteLine ($"Tests run: {testCount}, Passed: {passCount}, Errors: {errorCount}, Failures: {failureCount}, Inconclusive: {inconclusiveCount}");
		Console.WriteLine ($"Not run: {notRunCount}, Invalid: {invalidCount}, Ignored: {ignoreCount}, Skipped: {skipCount}");
		Dump (badTests, "Errors and Failures");
		Dump (skippedTests, "Not Run");
		Dump (inconclusiveTests, "Inconclusive");
	}

	int reportCount;

	void Dump (List<ITestResult> tests, string reason)
	{
		if (tests.Count == 0)
			return;
		
		Console.WriteLine ($"{reason}:");
		foreach (var result in tests) {
	        Console.WriteLine();
	        Console.WriteLine("{0}) {1} ({2})", ++reportCount, result.Name, result.FullName);

	        if (result.Message != null && result.Message != string.Empty)
	            Console.WriteLine("   {0}", result.Message);

	        if (result.StackTrace != null && result.StackTrace != string.Empty)
	            Console.WriteLine(result.ResultState == ResultState.Failure
	                ? StackFilter.Filter(result.StackTrace)
	                : result.StackTrace + NUnit.Env.NewLine);
		}
	}
}


public class IncrementalTestRunner {
	FinallyDelegate finallyDelegate;
	ITestAssemblyRunner runner;
	IDictionary loadOptions;
    ITestFilter filter = TestFilter.Empty;
	ResultCollector resultCollector;
	ITestFilter execFilter;
	CountingFilter countFilter;
	UnhandledExceptionEventHandler crashHandler;

	public IncrementalTestRunner () {
        this.finallyDelegate = new FinallyDelegate();
        this.runner = new NUnitLiteTestAssemblyRunner(new NUnitLiteTestAssemblyBuilder(), finallyDelegate);

		this.loadOptions = new Hashtable ();
	}

	public void Add (Assembly assembly) {
		if (!runner.Load (assembly, loadOptions))
			throw new Exception ("Could not load " + assembly);
	}

	public void Exclude (string categories) {
		var excludeFilter = new NotFilter (new SimpleCategoryExpression(categories).Filter);
		filter = And (filter, excludeFilter);
		
	}

	public void Start (int step) {
		if (resultCollector != null)
			throw new Exception ("Test already started");
		
		resultCollector = new ResultCollector ();
		countFilter = new CountingFilter (step);
		execFilter = And (filter, countFilter);

        crashHandler = new UnhandledExceptionEventHandler(TopLevelHandler);
        AppDomain.CurrentDomain.UnhandledException += crashHandler;
	}

	public bool Step () {
		ITestResult result = runner.Run(new MyListener (), execFilter);
		if (!resultCollector.Update (result)) {
			Console.WriteLine ();
			resultCollector.Dump ();
			AppDomain.CurrentDomain.UnhandledException -= crashHandler;

			return false;
		}

		countFilter.Bump ();
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
