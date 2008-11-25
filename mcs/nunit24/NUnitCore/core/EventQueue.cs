// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;
using System.Collections;
using System.Threading;

namespace NUnit.Core
{
	#region Individual Event Classes

	/// <summary>
	/// NUnit.Core.Event is the abstract base for all stored events.
	/// An Event is the stored representation of a call to the 
	/// EventListener interface and is used to record such calls
	/// or to queue them for forwarding on another thread or at
	/// a later time.
	/// </summary>
	public abstract class Event
	{
		abstract public void Send( EventListener listener );
	}

	public class RunStartedEvent : Event
	{
		string name;
		int testCount;

		public RunStartedEvent( string name, int testCount )
		{
			this.name = name;
			this.testCount = testCount;
		}

		public override void Send( EventListener listener )
		{
			listener.RunStarted(name, testCount);
		}
	}

	public class RunFinishedEvent : Event
	{
		TestResult result;
		Exception exception;

		public RunFinishedEvent( TestResult result )
		{
			this.result = result;
		}

		public RunFinishedEvent( Exception exception )
		{
			this.exception = exception;
		}

		public override void Send( EventListener listener )
		{
			if ( this.exception != null )
				listener.RunFinished( this.exception );
			else
				listener.RunFinished( this.result );
		}
	}

	public class TestStartedEvent : Event
	{
		TestName testName;

		public TestStartedEvent( TestName testName )
		{
			this.testName = testName;
		}

		public override void Send( EventListener listener )
		{
			listener.TestStarted( this.testName );
		}
	}
			
	public class TestFinishedEvent : Event
	{
		TestCaseResult result;

		public TestFinishedEvent( TestCaseResult result )
		{
			this.result = result;
		}

		public override void Send( EventListener listener )
		{
			listener.TestFinished( this.result );
		}
	}

	public class SuiteStartedEvent : Event
	{
		TestName suiteName;

		public SuiteStartedEvent( TestName suiteName )
		{
			this.suiteName = suiteName;
		}

		public override void Send( EventListener listener )
		{
			listener.SuiteStarted( this.suiteName );
		}
	}

	public class SuiteFinishedEvent : Event
	{
		TestSuiteResult result;

		public SuiteFinishedEvent( TestSuiteResult result )
		{
			this.result = result;
		}

		public override void Send( EventListener listener )
		{
			listener.SuiteFinished( this.result );
		}
	}

	public class UnhandledExceptionEvent : Event
	{
		Exception exception;

		public UnhandledExceptionEvent( Exception exception )
		{
			this.exception = exception;
		}

		public override void Send( EventListener listener )
		{
			listener.UnhandledException( this.exception );
		}
	}

	public class OutputEvent : Event
	{
		TestOutput output;

		public OutputEvent( TestOutput output )
		{
			this.output = output;
		}

		public override void Send( EventListener listener )
		{
			listener.TestOutput( this.output );
		}
	}

	#endregion

	/// <summary>
	/// Implements a queue of work items each of which
	/// is queued as a WaitCallback.
	/// </summary>
	public class EventQueue
	{
		private Queue queue = new Queue();

		public int Count
		{
			get 
			{
				lock( this )
				{
					return this.queue.Count; 
				}
			}
		}

		public void Enqueue( Event e )
		{
			lock( this )
			{
				this.queue.Enqueue( e );
				Monitor.Pulse( this );
			}
		}

		public Event Dequeue()
		{
			lock( this )
			{
				return (Event)this.queue.Dequeue();
			}
		}
	}
}
