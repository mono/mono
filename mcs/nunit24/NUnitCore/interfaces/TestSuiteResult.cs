// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Core
{
	using System;
	using System.Collections;

	/// <summary>
	/// TestSuiteResult represents the result of running a 
	/// TestSuite. It adds a set of child results to the
	/// base TestResult class.
	/// </summary>
	/// 
	[Serializable]
	public class TestSuiteResult : TestResult
	{
		private ArrayList results = new ArrayList();
		
		/// <summary>
		/// Construct a TestSuiteResult from a test and a name
		/// </summary>
		/// <param name="test"></param>
		/// <param name="name"></param>
		public TestSuiteResult(TestInfo test, string name) 
			: base(test, name) { }

		/// <summary>
		/// Construct a TestSuite result from a string
		/// 
		/// This overload is used for testing
		/// </summary>
		/// <param name="testSuiteString"></param>
		public TestSuiteResult(string testSuiteString) 
			: base(null, testSuiteString) { }

		/// <summary>
		/// Add a child result to a TestSuiteResult
		/// </summary>
		/// <param name="result">The child result to be added</param>
		public void AddResult(TestResult result) 
		{
			results.Add(result);

			if( this.ResultState == ResultState.Success &&
				result.ResultState != ResultState.Success )
			{
				this.Failure( "Child test failed", null, FailureSite.Child );
			}
		}

		/// <summary>
		/// Gets a list of the child results of this TestSUiteResult
		/// </summary>
		public IList Results
		{
			get { return results; }
		}

		/// <summary>
		/// Accepts a ResultVisitor
		/// </summary>
		/// <param name="visitor">The visitor</param>
		public override void Accept(ResultVisitor visitor) 
		{
			visitor.Visit(this);
		}
	}
}
