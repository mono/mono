// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;
using System.Collections;

namespace NUnit.Core.Extensibility
{
	/// <summary>
	/// EventListenerCollection holds multiple event listeners
	/// and relays all event calls to each of them.
	/// </summary>
	public class EventListenerCollection : ExtensionPoint, EventListener
	{
		#region Constructor
		public EventListenerCollection( IExtensionHost host )
			: base( "EventListeners", host ) { }
		#endregion

		#region EventListener Members
		public void RunStarted(string name, int testCount)
		{
			foreach( EventListener listener in extensions )
				listener.RunStarted( name, testCount );
		}

		public void RunFinished(TestResult result)
		{
			foreach( EventListener listener in extensions )
				listener.RunFinished( result );
		}

		public void RunFinished(Exception exception)
		{
			foreach( EventListener listener in extensions )
				listener.RunFinished( exception );
		}

		public void SuiteStarted(TestName testName)
		{
			foreach( EventListener listener in extensions )
				listener.SuiteStarted( testName );
		}

		public void SuiteFinished(TestSuiteResult result)
		{
			foreach( EventListener listener in extensions )
				listener.SuiteFinished( result );
		}

		public void TestStarted(TestName testName)
		{
			foreach( EventListener listener in extensions )
				listener.TestStarted( testName );
		}

		public void TestFinished(TestCaseResult result)
		{
			foreach( EventListener listener in extensions )
				listener.TestFinished( result );
		}

		public void UnhandledException(Exception exception)
		{
			foreach( EventListener listener in extensions )
				listener.UnhandledException( exception );
		}

		public void TestOutput(TestOutput testOutput)
		{
			foreach( EventListener listener in extensions )
				listener.TestOutput( testOutput );
		}

		#endregion

		#region ExtensionPoint Overrides
		protected override bool ValidExtension(object extension)
		{
			return extension is EventListener; 
		}
		#endregion
	}
}
