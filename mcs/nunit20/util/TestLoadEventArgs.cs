using System;
using System.Diagnostics;

namespace NUnit.Util
{
	/// <summary>
	/// The delegate used for all events related to loading, unloading and reloading tests
	/// </summary>
	public delegate void TestLoadEventHandler( object sender, TestLoadEventArgs e );

	/// <summary>
	/// Enumeration used to distinguish test load events
	/// </summary>
	public enum TestLoadAction
	{
		LoadStarting,
		LoadComplete,
		LoadFailed,
		ReloadStarting,
		ReloadComplete,
		ReloadFailed,
		UnloadStarting,
		UnloadComplete,
		UnloadFailed
	}

	/// <summary>
	/// Argument used for all test load events
	/// </summary>
	public class TestLoadEventArgs : EventArgs
	{
		private TestLoadAction action;
		private string assemblyName;
		private UITestNode test;
		private Exception exception;

		/// <summary>
		/// Helper that recognizes failure events
		/// </summary>
		private bool IsFailure( TestLoadAction action )
		{
			return action == TestLoadAction.LoadFailed ||
				action == TestLoadAction.UnloadFailed ||
				action == TestLoadAction.ReloadFailed;
		}

		/// <summary>
		/// Constructor for non-failure events
		/// </summary>
		public TestLoadEventArgs( TestLoadAction action, 
			string assemblyName, UITestNode test )
		{
			this.action = action;
			this.assemblyName = assemblyName;
			this.test = test;

			Debug.Assert( !IsFailure( action ), "Invalid TestLoadAction in Constructor" );
		}

		public TestLoadEventArgs( TestLoadAction action, string assemblyName )
		{
			this.action = action;
			this.assemblyName = assemblyName;

			Debug.Assert( action != TestLoadAction.UnloadStarting || action != TestLoadAction.UnloadComplete, 
					"Invalid TestLoadAction in Constructor" );
		}

		/// <summary>
		/// Constructor for failure events
		/// </summary>
		public TestLoadEventArgs( TestLoadAction action,
			string assemblyName, Exception exception )
		{
			this.action = action;
			this.assemblyName = assemblyName;
			this.exception = exception;

			Debug.Assert( IsFailure( action ), "Invalid TestLoadAction in Constructor" );
		}

		public TestLoadAction Action
		{
			get { return action; }
		}

		public string AssemblyName
		{
			get { return assemblyName; }
		}

		public UITestNode Test
		{
			get { return test; }
		}

		public Exception Exception
		{
			get { return exception; }
		}
	}
}
