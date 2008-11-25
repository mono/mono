// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Util
{
	using System;
	using NUnit.Core;

	/// <summary>
	/// SummaryVisitor examines a set of results and calculates 
	/// summary statistics for the run. Note that a test run
	/// will only produce results for tests that were selected
	/// to be run. Curently, tests excluded by the Explicit 
	/// attribute produce a result, while those excluded by
	/// the Platform attribute do not. This anomaly will be
	/// corrected in a later version.
	/// </summary>
	public class SummaryVisitor : ResultVisitor
	{
		private int resultCount;
		private int failureCount;
		private int skipCount;
		private int ignoreCount;
		private int suitesNotRun;
		
		private double time;
		private string name;
		private bool initialized;

		public SummaryVisitor()
		{
			resultCount = 0;
			initialized = false;
		}

		public void Visit(TestCaseResult caseResult) 
		{
			SetNameandTime(caseResult.Name, caseResult.Time);

			switch( caseResult.RunState )
			{
				case RunState.Executed:
					resultCount++;
					if(caseResult.IsFailure)
						failureCount++;
					break;
				case RunState.Ignored:
					ignoreCount++;
					break;
				case RunState.Explicit:
				case RunState.NotRunnable:
				case RunState.Runnable:
				case RunState.Skipped:
				default:
					skipCount++;
					break;
			}
		}

		public void Visit(TestSuiteResult suiteResult) 
		{
			SetNameandTime(suiteResult.Name, suiteResult.Time);

			
			
			foreach (TestResult result in suiteResult.Results)
			{
				result.Accept(this);
			}
			
			if(!suiteResult.Executed)
				suitesNotRun++;
		}

		public double Time
		{
			get { return time; }
		}

		private void SetNameandTime(string name, double time)
		{
			if(!initialized)
			{
				this.time = time;
				this.name = name;
				initialized = true;
			}
		}

		public bool Success
		{
			get { return (failureCount == 0); }
		}

		public int ResultCount
		{
			get { return resultCount; }
		}

		public int FailureCount
		{
			get { return failureCount; }
		}

		public int SkipCount
		{
			get { return skipCount; }
		}

		public int IgnoreCount
		{
			get { return ignoreCount; }
		}

		public int TestsNotRun
		{
			get { return skipCount + ignoreCount; }
		}

		public int SuitesNotRun
		{
			get { return suitesNotRun; }
		}

		public string Name
		{
			get { return name; }
		}
	}
}
