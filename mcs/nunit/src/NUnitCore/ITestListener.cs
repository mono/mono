namespace NUnit.Framework 
{
	using System;
  
	/// <summary>A Listener for test progress</summary>
	public interface ITestListener 
	{
		/// <summary>An error occurred.</summary>
		void AddError(ITest test, Exception ex);
    
		/// <summary>A failure occurred.</summary>
		void AddFailure(ITest test, AssertionFailedError ex);
    
		/// <summary>A test ended.</summary>
		void EndTest(ITest test); 
    
		/// <summary>A test started.</summary>
		void StartTest(ITest test);
	}
#if false
	public class TestEventArgs : System.EventArgs
	{
		private readonly ITest fTest;
		public TestEventArgs(ITest test) : base()
		{
			fTest = test;
		}
		public ITest Test
		{
			get { return fTest;}
		}
	}
	public class TestErrorArgs : TestEventArgs
	{
		private readonly Exception fThrownException;
		
		public TestErrorArgs(ITest test, Exception thrownException) : base(test)
		{
			fThrownException = thrownException;
		}
		
		public TestErrorArgs(TestFailure error)
			: this(error.FailedTest,error.ThrownException){}
		
		public Exception ThrownException
		{
			get { return fThrownException;}
			
		}
	}

	public delegate void TestErrorHandler(TestFailure failure);
	public delegate void TestEventHandler(ITest test);

	public interface ITestEvents
	{
		event TestErrorHandler TestErred;
		event TestErrorHandler TestFailed;
		event TestEventHandler TestStarted;
		event TestEventHandler TestEnded;
		event TestEventHandler RunStarted;
		event TestEventHandler RunEnded;
	}
#endif
}