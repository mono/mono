#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright  2000-2002 Philip A. Craig
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
' Portions Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright  2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;
using System.Diagnostics;
using NUnit.Core;

namespace NUnit.Util
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
		// Project Loading Events
		ProjectLoading,
		ProjectLoaded,
		ProjectLoadFailed,
		ProjectUnloading,
		ProjectUnloaded,
		ProjectUnloadFailed,
		// Test Loading Events
		TestLoading,
		TestLoaded,
		TestLoadFailed,
		TestReloading,
		TestReloaded,
		TestReloadFailed,
		TestUnloading,
		TestUnloaded,
		TestUnloadFailed,
		// Test Running Events
		RunStarting,
		RunFinished,
		SuiteStarting,
		SuiteFinished,
		TestStarting,
		TestFinished
	}
	
	/// <summary>
	/// Argument used for all test events
	/// </summary>
	public class TestEventArgs : EventArgs
	{
		#region Instance Variables

		// The action represented by the event
		private TestAction action;

		// The file name for the test
		private string testFileName;
		
		// The top node of a loaded test suite
		private UITestNode test;

		// The result of a test
		private TestResult result;
		
		// The exception causing a failure
		private Exception exception;

		#endregion

		#region Constructors

		public TestEventArgs( TestAction action, 
			string testFileName, UITestNode test )
		{
			this.action = action;
			this.testFileName = testFileName;
			this.test = test;
		}

		public TestEventArgs( TestAction action, string testFileName )
		{
			this.action = action;
			this.testFileName = testFileName;
		}

		public TestEventArgs( TestAction action,
			string testFileName, Exception exception )
		{
			this.action = action;
			this.testFileName = testFileName;
			this.exception = exception;
		}

		public TestEventArgs( TestAction action, UITestNode test )
		{
			this.action = action;
			this.test = test;
		}

		public TestEventArgs( TestAction action, TestResult result )
		{
			this.action = action;
			this.result = result;
		}

		public TestEventArgs( TestAction action, Exception exception )
		{
			this.action = action;
			this.exception = exception;
		}

		#endregion

		#region Properties

		public TestAction Action
		{
			get { return action; }
		}

		public string TestFileName
		{
			get { return testFileName; }
		}

		public bool IsProjectFile
		{
			get { return NUnitProject.IsProjectFile( testFileName ); }
		}

		public UITestNode Test
		{
			get { return test; }
		}

		public TestResult Result
		{
			get { return result; }
		}

		public Exception Exception
		{
			get { return exception; }
		}

		#endregion
	}
}
