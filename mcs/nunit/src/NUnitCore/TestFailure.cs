namespace NUnit.Framework 
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Text;
  
	/// <summary>
	/// A <c>TestFailure</c> collects a failed test together with
	/// the caught exception.
	/// </summary>
	/// <seealso cref="TestResult"/>
	public class TestFailure : MarshalByRefObject
	{
		private readonly ITest fFailedTest;
		private readonly Exception fThrownException;

		/// <summary>
		/// Constructs a TestFailure with the given test and exception.
		/// </summary>
		public TestFailure(ITest theFailedTest, Exception theThrownException) 
		{
			if(theFailedTest==null)
				throw new ArgumentNullException("theFailedTest");
			if(theThrownException==null)
				throw new ArgumentNullException("theThrownException");
			this.fFailedTest = theFailedTest;
			this.fThrownException = theThrownException;
		}

		/// <value>Gets the failed test.</value>
		public ITest FailedTest
		{ 
			get { return this.fFailedTest; }
		}

		/// <value>True if it's a failure, false if error.</value>
		public bool IsFailure 
		{
			get { return this.fThrownException is AssertionFailedError; }
		}

		/// <value>Gets the thrown exception.</value>
		public Exception ThrownException 
		{
			get { return this.fThrownException; }
		}

		/// <summary>Returns a short description of the failure.</summary>
		public override string ToString() 
		{
			return this.fFailedTest + ": " + this.fThrownException.Message;
		}
	}
}