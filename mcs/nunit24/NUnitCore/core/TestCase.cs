// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Core
{
	using System;
	using System.Collections;
	using System.Reflection;

	/// <summary>
	/// The abstract TestCase class represents a single test case.
	/// In the present implementation, the only derived class is
	/// TestMethod, but we allow for future test cases which are
	/// implemented in other ways.
	/// </summary>
	public abstract class TestCase : Test
	{
		public TestCase( string path, string name ) : base( path, name ) { }

		public TestCase( MethodInfo method ) : base ( method ) { }

		public TestCase( TestName testName ) : base ( testName ) { }

		public override int CountTestCases( ITestFilter filter ) 
		{
			if (filter.Pass(this))
				return 1;

			return 0;
		}

		protected virtual TestCaseResult MakeTestCaseResult()
		{
			return new TestCaseResult( new TestInfo(this) );
		}

		public override TestResult Run(EventListener listener, ITestFilter filter)
		{
			return Run( listener ); // Ignore filter for now
		}

		public override TestResult Run( EventListener listener )
		{
			using( new TestContext() )
			{
				TestCaseResult testResult = MakeTestCaseResult();

				listener.TestStarted( this.TestName );
				long startTime = DateTime.Now.Ticks;

				switch (this.RunState)
				{
					case RunState.Runnable:
					case RunState.Explicit:
						Run(testResult);
						break;
					case RunState.Skipped:
						testResult.Skip(IgnoreReason);
						break;
					default:
					case RunState.NotRunnable:
					case RunState.Ignored:
						testResult.Ignore(IgnoreReason);
						break;
				}

				long stopTime = DateTime.Now.Ticks;
				double time = ((double)(stopTime - startTime)) / (double)TimeSpan.TicksPerSecond;
				testResult.Time = time;

				listener.TestFinished(testResult);
				return testResult;
			}
		}

		public override string TestType
		{
			get { return "Test Case"; }
		}

		public override bool IsSuite
		{
			get { return false; }
		}

		public override IList Tests
		{
			get { return null; }
		}

		public abstract void Run(TestCaseResult result);
	}
}
