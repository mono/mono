// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System.Collections;

namespace NUnit.Core
{
	/// <summary>
	/// Common interface supported by all representations
	/// of a test. Only includes informational fields.
	/// The Run method is specifically excluded to allow
	/// for data-only representations of a test.
	/// </summary>
	public interface ITest
    {
        #region Properties
        /// <summary>
		/// Gets the completely specified name of the test
		/// encapsulated in a TestName object.
		/// </summary>
		TestName TestName { get; }

		/// <summary>
		/// Gets a string representing the type of test, e.g.: "Test Case"
		/// </summary>
		string TestType { get; }

        /// <summary>
        /// Indicates whether the test can be run using
        /// the RunState enum.
        /// </summary>
		RunState RunState { get; set; }

		/// <summary>
		/// Reason for not running the test, if applicable
		/// </summary>
		string IgnoreReason { get; set; }
		
		/// <summary>
		/// Count of the test cases ( 1 if this is a test case )
		/// </summary>
		int TestCount { get; }

		/// <summary>
		/// Categories available for this test
		/// </summary>
		IList Categories { get; }

		/// <summary>
		/// Return the description field. 
		/// </summary>
		string Description { get; set; }

		/// <summary>
		/// Return additional properties of the test
		/// </summary>
		IDictionary Properties { get; }

		/// <summary>
		/// True if this is a suite
		/// </summary>
		bool IsSuite { get; }

		/// <summary>
		///  Gets the parent test of this test
		/// </summary>
		ITest Parent { get; }

		/// <summary>
		/// For a test suite, the child tests or suites
		/// Null if this is not a test suite
		/// </summary>
		IList Tests { get; }
        #endregion

        #region Methods
		/// <summary>
		/// Count the test cases that pass a filter. The
		/// result should match those that would execute
		/// when passing the same filter to Run.
		/// </summary>
		/// <param name="filter">The filter to apply</param>
		/// <returns>The count of test cases</returns>
        int CountTestCases(ITestFilter filter);
        #endregion
    }
}

