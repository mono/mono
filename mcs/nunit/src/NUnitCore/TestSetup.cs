namespace NUnit.Extensions 
{
	using System;
	using NUnit.Framework;
  
	/// <summary>A Decorator to set up and tear down additional fixture state.
	/// </summary><remarks>
	/// Subclass TestSetup and insert it into your tests when you want
	/// to set up additional state once before the tests are run.</remarks>
	public class TestSetup: TestDecorator 
	{
		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		/// <param name="test"></param>
		public TestSetup(ITest test) : base(test) {}
		#endregion

		#region Nested Classes
		/// <summary>
		/// 
		/// </summary>
		protected class ProtectedProtect: IProtectable 
		{
			private readonly TestSetup fTestSetup;
			private readonly TestResult fTestResult;
			/// <summary>
			/// 
			/// </summary>
			/// <param name="testSetup"></param>
			/// <param name="testResult"></param>
			public ProtectedProtect(TestSetup testSetup, TestResult testResult)
			{
				if(testSetup == null)
					throw new ArgumentNullException("testSetup");
				if(testResult == null)
					throw new ArgumentNullException("testResult");
				this.fTestSetup = testSetup;
				this.fTestResult = testResult;
			}
			/// <summary>
			/// 
			/// </summary>
			void IProtectable.Protect() 
			{
				this.fTestSetup.SetUp();
				this.fTestSetup.BasicRun(fTestResult);
				this.fTestSetup.TearDown();
			}
		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="result"></param>
		public override void Run(TestResult result) 
		{
			IProtectable p = new ProtectedProtect(this, result);
			result.RunProtected(this, p);
		}
		/// <summary>Sets up the fixture. Override to set up additional fixture
		/// state.</summary>
		protected virtual void SetUp() {}
		/// <summary>Tears down the fixture. Override to tear down the additional
		/// fixture state.</summary>
		protected virtual void TearDown() {}
	}
}
