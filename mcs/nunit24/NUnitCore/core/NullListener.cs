// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;

namespace NUnit.Core
{
	/// <summary>
	/// Summary description for NullListener.
	/// </summary>
	/// 
	[Serializable]
	public class NullListener : EventListener
	{
		public void RunStarted( string name, int testCount ){ }

		public void RunFinished( TestResult result ) { }

		public void RunFinished( Exception exception ) { }

		public void TestStarted(TestName testName){}
			
		public void TestFinished(TestCaseResult result){}

		public void SuiteStarted(TestName testName){}

		public void SuiteFinished(TestSuiteResult result){}

		public void UnhandledException( Exception exception ) {}

		public void TestOutput(TestOutput testOutput) {}

		public static EventListener NULL
		{
			get { return new NullListener();}
		}
	}
}
