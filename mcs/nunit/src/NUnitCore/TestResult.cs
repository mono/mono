namespace NUnit.Framework 
{
	using System;
	using System.Collections;
	using System.Threading;

	/// <summary>A <c>TestResult</c> collects the results of executing
	/// a test case. It is an instance of the Collecting Parameter pattern.
	/// </summary><remarks>
	/// The test framework distinguishes between failures and errors.
	/// A failure is anticipated and checked for with assertions. Errors are
	/// unanticipated problems like an <c>ArgumentOutOfRangeException</c>.
	/// </remarks><seealso cref="ITest"/>
	public class TestResult : MarshalByRefObject
	{
		#region Instance Variables
		private ArrayList fFailures;
		private ArrayList fErrors;
		private ArrayList fListeners;
		private int fRunTests;
		private bool fStop;
		#endregion
		
		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		public TestResult() 
		{
			fFailures= new ArrayList();
			fErrors= new ArrayList();
			fListeners= new ArrayList();
		}
		#endregion

		#region Collection Methods
		/// <summary>
		/// Adds an error to the list of errors. The passed in exception
		/// caused the error.
		/// </summary>
		public void AddError(ITest test, Exception error) 
		{
			lock(this)
			{
				this.fErrors.Add(new TestFailure(test, error));
				foreach (ITestListener listner in CloneListeners()) 
				{
					listner.AddError(test, error);
				}
			}
		}
		/// <summary>
		/// Adds a failure to the list of failures. The passed in
		/// exception caused the failure.
		/// </summary>
		public void AddFailure(ITest test, AssertionFailedError failure)
		{
			lock(this)
			{
				fFailures.Add(new TestFailure(test, failure));
				foreach (ITestListener listner in CloneListeners()) 
				{
					listner.AddFailure(test, failure);
				}
			}
		}
		#endregion

		#region Events
		/// <summary>Registers a TestListener.</summary>
		public void AddListener(ITestListener listener) 
		{
			lock(this) 
				this.fListeners.Add(listener);
		}
		/// <summary>Unregisters a TestListener</summary>
		public void RemoveListener(ITestListener listener) 
		{
			lock(this) 
			{
				fListeners.Remove(listener);
			}
		}
		/// <summary>Returns a copy of the listeners.</summary>
		private ArrayList CloneListeners() 
		{
			lock(this)
			{
				return (ArrayList)fListeners.Clone();
			}
		}
		/// <summary>Informs the result that a test was completed.</summary>
		public void EndTest(ITest test) 
		{
			foreach (ITestListener listner in CloneListeners()) 
			{
				listner.EndTest(test);
			}
		}
		/// <summary>Informs the result that a test will be started.</summary>
		public void StartTest(ITest test) 
		{
			lock(this)
			{
				this.fRunTests += test.CountTestCases;
			}
			foreach (ITestListener listner in CloneListeners()) 
			{
				listner.StartTest(test);
			}
		}
		#endregion

		#region Properties
		/// <value>Gets the number of run tests.</value>
		public int RunCount 
		{
			get {lock(this)return this.fRunTests; }
		}
		/// <value>Gets the number of detected errors.</value>
		public int ErrorCount 
		{
			get {lock(this)return this.fErrors.Count; }
		}  
		/// <value>Gets the number of detected failures.</value>
		public int FailureCount 
		{
			get {lock(this)return this.fFailures.Count; }
		}
		/// <summary>Checks whether the test run should stop.</summary>
		public bool ShouldStop 
		{
			get {lock(this)return this.fStop; }
		}
		/// <value>Returns whether the entire test was successful or not.</value>
		public bool WasSuccessful 
		{
			get 
			{
				lock(this)
				{
					return (this.FailureCount == 0)
						&& (this.ErrorCount == 0);
				}
			}
		}
		/// <value>Returns a TestFailure[] for the errors.</value>
		public TestFailure[] Errors
		{
			get
			{ 
				lock(this)
				{
					TestFailure[] retVal = new TestFailure[this.fErrors.Count];
					this.fErrors.CopyTo(retVal);
					return retVal;
				}	
			}
		}
		/// <value>Returns a TestFauiler[] for the failures.</value>
		public TestFailure[] Failures 
		{
			get
			{
				lock(this)
				{
					TestFailure[] retVal = new TestFailure[this.fFailures.Count];
					this.fFailures.CopyTo(retVal);
					return retVal;
				}
			}
		}
		#endregion
		
		#region Nested Classes
		/// <summary>Runs a TestCase.</summary>
		protected class ProtectedProtect: IProtectable 
		{
			private TestCase fTest;
			/// <summary>
			/// 
			/// </summary>
			/// <param name="test"></param>
			public ProtectedProtect(TestCase test) 
			{
				if(test != null)
				{
					this.fTest = test;
				}
				else
				{
					throw new ArgumentNullException("test");
				}
			}
			/// <summary>
			/// 
			/// </summary>
			public void Protect() 
			{
				this.fTest.RunBare();
			}
		}
		#endregion

		#region Run Methods
		/// <summary>Runs a TestCase.</summary>
		internal void Run(TestCase test) 
		{
			StartTest(test);
			IProtectable p = new ProtectedProtect(test);
			RunProtected(test, p);
			EndTest(test);
		}

		/// <summary>Runs a TestCase.</summary>
		public void RunProtected(ITest test, IProtectable p) 
		{
			try 
			{
				p.Protect();
			}
			catch (AssertionFailedError e) 
			{
				AddFailure(test, e);
			}
			catch (NUnitException e) 
			{
				if (e.IsAssertionFailure)
					AddFailure(test, (AssertionFailedError)e.InnerException);
				else
					AddError(test, e.InnerException);
			}
			catch (ThreadAbortException e) 
			{ // don't catch by accident
				throw e;
			}
			catch (System.Exception e) 
			{
				AddError(test, e);
			}
		}
		/// <summary>Marks that the test run should stop.</summary>
		public void Stop() 
		{
			fStop= true;
		}
		#endregion
	}
}