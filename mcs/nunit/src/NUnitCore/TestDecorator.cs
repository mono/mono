namespace NUnit.Extensions 
{
	using System;
	using NUnit.Framework;

	/// <summary>A Decorator for Tests.</summary>
	/// <remarks>Use TestDecorator as the base class
	/// for defining new test decorators. TestDecorator subclasses
	/// can be introduced to add behaviour before or after a test
	/// is run.</remarks>
	public class TestDecorator: Assertion, ITest 
	{
		/// <summary>
		/// A reference to the test that is being decorated
		/// </summary>
		protected readonly ITest fTest;
		/// <summary>
		/// Creates a decorator for the supplied test
		/// </summary>
		/// <param name="test">The test to be decorated</param>
		public TestDecorator(ITest test) 
		{
			if(test!= null)
			{
				this.fTest= test;
			}
			else
				throw new ArgumentNullException("test");
		}
    	/// <summary>
		/// 
		/// </summary>
		/// <param name="result"></param>
		public virtual void Run(TestResult result) 
		{
			this.BasicRun(result);
		}
		/// <summary>The basic run behaviour.</summary>
		public void BasicRun(TestResult result) 
		{
			this.fTest.Run(result);
		}
		/// <summary>
		/// 
		/// </summary>
		public virtual int CountTestCases 
		{
			get { return fTest.CountTestCases; }
		}
		/// <summary>
		/// 
		/// </summary>
		public ITest GetTest 
		{
			get { return fTest; }
		}
		//public string Name
		//{
		//	get{return fTest.Name;}
		//}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString() 
		{
			return fTest.ToString();
		}
	}
}