#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright © 2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright © 2000-2003 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright © 2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright © 2000-2003 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;

namespace NUnit.Core
{
	/// <summary>
	/// The delegate for all events related to running tests
	/// </summary>
	public delegate void TestEventHandler ( object sender, TestEventArgs args );

	/// <summary>
	/// Enumeration used to distiguish test events
	/// </summary>
	public enum TestAction
	{
		// Test Load Events
		TestLoading,
		TestLoaded,
		TestLoadFailed,
		TestReloading,
		TestReloaded,
		TestReloadFailed,
		TestUnloading,
		TestUnloaded,
		TestUnloadFailed,
		// Test Run Events
		RunStarting,
		RunFinished,
		SuiteStarting,
		SuiteFinished,
		TestStarting,
		TestFinished,
		TestException
	}

	/// <summary>
	/// Argument used for all test events
	/// </summary>
	public class TestEventArgs : EventArgs
	{
		#region Instance Variables

		// The action represented by the event
		private TestAction action;

		// The name of the test or other item
		private string name;
		
		// The tests we are running
		private Test[] tests;

		// The results from our tests
		private TestResult[] results;
		
		// The exception causing a failure
		private Exception exception;

		// TODO: Remove this count of test cases
		private int count;

		#endregion

		#region Constructors

		public TestEventArgs( TestAction action, 
			string name, Test test )
		{
			this.action = action;
			this.name = name;
			this.tests = new Test[] { test };
		}

		public TestEventArgs( TestAction action, string name )
		{
			this.action = action;
			this.name = name;
		}

		public TestEventArgs( TestAction action,
			string name, Exception exception )
		{
			this.action = action;
			this.name = name;
			this.exception = exception;
		}

		public TestEventArgs( TestAction action, Test test )
		{
			this.action = action;
			this.tests = new Test[] { test };
			this.count = test.CountTestCases();
		}

		public TestEventArgs( TestAction action, TestResult result )
		{
			this.action = action;
			this.results = new TestResult[] { result };
		}

		public TestEventArgs( TestAction action, TestResult[] results )
		{
			this.action = action;
			this.results = results;
		}

		public TestEventArgs( TestAction action, Exception exception )
		{
			this.action = action;
			this.exception = exception;
		}

		public TestEventArgs( TestAction action, Test[] tests, int count) 
		{
			this.action = action;
			this.tests = tests;
			this.count = count;
		}

		#endregion

		#region Properties

		public TestAction Action
		{
			get { return action; }
		}

		public string Name
		{
			get { return name; }
		}

//		public bool IsProjectFile
//		{
//			get { return NUnitProject.IsProjectFile( testFileName ); }
//		}

		public Test Test
		{
			get { return tests == null || tests.Length == 0 ? null : tests[0]; }
		}

		public Test[] Tests 
		{
			get { return tests; }
		}

		public int TestCount 
		{
			get { return count; }
		}

		public TestResult Result
		{
			get { return results == null || results.Length == 0 ? null : results[0]; }
		}

		public TestResult[] Results
		{
			get { return results; }
		}

		public Exception Exception
		{
			get { return exception; }
		}

		#endregion
	}
}
