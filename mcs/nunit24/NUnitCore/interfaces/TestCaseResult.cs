// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Core
{
	using System;
	using System.Text;

	/// <summary>
	/// TestCaseResult represents the result of a test case execution
	/// </summary>
	[Serializable]
	public class TestCaseResult : TestResult
	{
        /// <summary>
        /// Construct a result for a test case
        /// </summary>
        /// <param name="testCase">The test case for which this is a result</param>
		public TestCaseResult(TestInfo testCase)
			: base(testCase, testCase.TestName.FullName) { }

		/// <summary>
		/// Construct a result from a string - used for tests
		/// </summary>
		/// <param name="testCaseString"></param>
		public TestCaseResult(string testCaseString) 
			: base(null, testCaseString) { }

        /// <summary>
        /// Accept a ResultVisitor
        /// </summary>
        /// <param name="visitor">The visitor to accept</param>
		public override void Accept(ResultVisitor visitor) 
		{
			visitor.Visit(this);
		}
	}
}
