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

namespace NUnit.Core
{
	using System;

	/// <summary>
	/// Summary description for TestResult.
	/// </summary>
	/// 
	[Serializable]
	public abstract class TestResult
	{
		#region Fields

		/// <summary>
		/// True if the test executed
		/// </summary>
		private bool executed;

		/// <summary>
		/// True if the test was marked as a failure
		/// </summary>
		private bool isFailure; 

		/// <summary>
		/// True if the setup failed: This means SetUp for a test case,
		/// or TestFixtureSetUp for a fixture.
		/// </summary>
		private bool setupFailure;
		
		/// <summary>
		/// The elapsed time for executing this test
		/// </summary>
		private double time;

		/// <summary>
		/// The name of the test
		/// </summary>
		private string name;

		/// <summary>
		/// The test that this result pertains to
		/// </summary>
		private ITest test;

		/// <summary>
		/// The stacktrace at the point of failure
		/// </summary>
		private string stackTrace;

		/// <summary>
		/// Description of this test
		/// </summary>
		private string description;

		/// <summary>
		/// Message giving the reason for failure
		/// </summary>
		protected string messageString;

		/// <summary>
		/// Number of asserts executed by this test
		/// </summary>
		private int assertCount;

		#endregion

		#region Protected Constructor

		protected TestResult(ITest test, string name)
		{
			this.name = name;
			this.test = test;
			if(test != null)
				this.description = test.Description;
		}

		#endregion

		#region Properties

		public bool Executed 
		{
			get { return executed; }
			set { executed = value; }
		}

		public virtual bool AllTestsExecuted
		{
			get { return executed; }
		}

		public virtual string Name
		{
			get{ return name;}
		}

		public ITest Test
		{
			get{ return test;}
		}

		public virtual bool IsSuccess
		{
			get { return !(isFailure); }
		}
		
		public virtual bool IsFailure
		{
			get { return isFailure; }
			set { isFailure = value; }
		}

		public bool SetupFailure
		{
			get { return setupFailure; }
			set { setupFailure = value; }
		}

		public virtual string Description
		{
			get { return description; }
			set { description = value; }
		}

		public double Time 
		{
			get{ return time; }
			set{ time = value; }
		}

		public string Message
		{
			get { return messageString; }
		}

		public virtual string StackTrace
		{
			get 
			{ 
				return stackTrace;
			}
			set 
			{
				stackTrace = value;
			}
		}

		public int AssertCount
		{
			get { return assertCount; }
			set { assertCount = value; }
		}

		#endregion

		#region Public Methods

		public void NotRun(string reason)
		{
			this.executed = false;
			this.messageString = reason;
		}

		public void Failure(string message, string stackTrace )
		{
			Failure( message, stackTrace, false );
		}

		public void Failure(string message, string stackTrace, bool setupFailure)
		{
			this.executed = true;
			this.isFailure = true;
			this.messageString = message;
			this.stackTrace = stackTrace;
			this.setupFailure = setupFailure;
		}

		#endregion

		public abstract void Accept(ResultVisitor visitor);
	}
}
