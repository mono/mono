namespace NUnit.Extensions 
{
	using System;
	using NUnit.Framework;

	/// <summary>A TestCase that expects an Exception of class fExpected
	/// to be thrown.</summary>
	/// <remarks> The other way to check that an expected exception is thrown is:
	/// <code>
	/// try {
	///   ShouldThrow();
	/// }catch (SpecialException) {
	///   return;
	/// }
	/// Assertion.Fail("Expected SpecialException");
	/// </code>
	///
	/// To use ExceptionTestCase, create a TestCase like:
	/// <code>
	/// new ExceptionTestCase("TestShouldThrow", typeof(SpecialException));
	/// </code></remarks>
	public class ExceptionTestCase: TestCase 
	{
		private readonly Type fExpected;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="exception"></param>
		public ExceptionTestCase(string name, Type exception) : base(name) 
		{
			fExpected= exception;
		}

		/// <summary>Execute the test method expecting that an Exception of
		/// class fExpected or one of its subclasses will be thrown.</summary>
		protected override void RunTest() 
		{
			try 
			{
				base.RunTest();
			}
			catch (Exception e) 
			{
				if (fExpected.IsAssignableFrom(e.InnerException.GetType()))
					return;
				else
					throw e.InnerException;
			}
			Assertion.Fail("Expected exception " + fExpected);
		}
	}
}
