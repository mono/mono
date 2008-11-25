// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System.Reflection;

namespace NUnit.Core.Extensibility
{
	/// <summary>
	/// The ITestCaseBuilder interface is exposed by a class that knows how to
	/// build a test case from certain methods. 
	/// </summary>
	public interface ITestCaseBuilder
	{
		/// <summary>
		/// Examine the method and determine if it is suitable for
		/// this builder to use in building a TestCase.
		/// 
		/// Note that returning false will cause the method to be ignored 
		/// in loading the tests. If it is desired to load the method
		/// but label it as non-runnable, ignored, etc., then this
		/// method must return true.
		/// 
		/// Derived classes must override this method.
		/// </summary>
		/// <param name="method">The test method to examine</param>
		/// <returns>True is the builder can use this method</returns>
		bool CanBuildFrom( MethodInfo method );

		/// <summary>
		/// Build a TestCase from the provided MethodInfo.
		/// </summary>
		/// <param name="method">The method to be used as a test case</param>
		/// <returns>A TestCase or null</returns>
		Test BuildFrom( MethodInfo method );
	}
}
