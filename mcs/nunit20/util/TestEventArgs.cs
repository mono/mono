using System;

namespace NUnit.Util
{
	/// <summary>
	/// The delegate for all events related to loading test projects
	/// </summary>
	public delegate void TestProjectEventHandler ( object sender, TestProjectEventArgs args );

	public enum TestProjectAction
	{
		ProjectLoading,
		ProjectLoaded,
		ProjectLoadFailed,
		ProjectUnloading,
		ProjectUnloaded,
		ProjectUnloadFailed,
	}

	/// <summary>
	/// Summary description for TestProjectEventArgs.
	/// </summary>
	public class TestProjectEventArgs : EventArgs
	{
		#region Instance Variables

		// The action represented by the event
		private TestProjectAction action;

		// The project name
		private string projectName;
		
		// The exception causing a failure
		private Exception exception;

		#endregion

		#region Constructors

		public TestProjectEventArgs( TestProjectAction action, string projectName )
		{
			this.action = action;
			this.projectName = projectName;
		}

		public TestProjectEventArgs( TestProjectAction action,
			string projectName, Exception exception )
		{
			this.action = action;
			this.projectName = projectName;
			this.exception = exception;
		}

		#endregion

		#region Properties

		public TestProjectAction Action
		{
			get { return action; }
		}

		public string ProjectName
		{
			get { return projectName; }
		}

		public Exception Exception
		{
			get { return exception; }
		}

		#endregion
	}
}
