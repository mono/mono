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
		private TestAction action;
		private UITestNode test;
		private TestResult result;
		private Exception exception;

		/// <summary>
		/// Helper to distinguish XxxxStarting from XxxxFinished actions
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		private static bool IsStartAction( TestAction action )
		{
			return action == TestAction.RunStarting ||
				action == TestAction.SuiteStarting ||
				action == TestAction.TestStarting;
		}

		/// <summary>
		/// Construct using action and test node
		/// Used only for XxxxStarting events
		/// </summary>
		public TestEventArgs( TestAction action, UITestNode test )
		{
			this.action = action;
			this.test = test;
			this.result = null;
			this.exception = null;

			Debug.Assert( IsStartAction( action ), "Invalid TestAction in Constructor" );
		}

		/// <summary>
		/// Construct using action and test result
		/// Used only for XxxxFinished events
		/// </summary>
		public TestEventArgs( TestAction action, TestResult result )
		{
			this.action = action;
			this.test = null;
			this.result = result;
			this.exception = null;

			Debug.Assert( !IsStartAction( action ), "Invalid TestAction in Constructor" );
		}

		/// <summary>
		/// Construct using action and exception
		/// Used only for RunFinished event
		/// </summary>
		public TestEventArgs( TestAction action, Exception exception )
		{
			this.action = action;
			this.test = null;
			this.result = null;
			this.exception = exception;

			Debug.Assert( action == TestAction.RunFinished, "Invalid TestAction in Constructor" );
		}

		/// <summary>
		/// The action that triggered the event
		/// </summary>
		public TestAction Action
		{
			get { return action; }
		}

		/// <summary>
		/// Test associated with a starting event
		/// </summary>
		public UITestNode Test
		{
			get { return test; }
		}

		/// <summary>
		/// Result associated with a finished event
		/// </summary>
		public TestResult Result
		{
			get { return result; }
		}

		/// <summary>
		/// Exception associated with a RunFinished event
		/// when caused by a system error or user cancelation
		/// </summary>
		public Exception Exception
		{
			get { return exception; }
		}
	}
}
