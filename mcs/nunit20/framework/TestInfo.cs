namespace NUnit.Core
{
	using System;
	using System.Collections;

	/// <summary>
	/// Common interface supported by all representations
	/// of a test. Only includes informational fields.
	/// The Run method is specifically excluded to allow
	/// for data-only representations of a test.
	/// </summary>
	public interface TestInfo
	{
		/// <summary>
		/// Name of the test
		/// </summary>
		string Name	{ get; }
		
		/// <summary>
		/// Full Name of the test
		/// </summary>
		string FullName { get; }

		/// <summary>
		/// Whether or not the test should be run
		/// </summary>
		bool ShouldRun { get; set; }

		/// <summary>
		/// Reason for not running the test, if applicable
		/// </summary>
		string IgnoreReason { get; set; }
		
		/// <summary>
		/// Count of the test cases ( 1 if this is a test case )
		/// </summary>
		int CountTestCases { get; }

		/// <summary>
		/// For a test suite, the child tests or suites
		/// Null if this is not a test suite
		/// </summary>
		ArrayList Tests { get; }

		/// <summary>
		/// True if this is a suite
		/// </summary>
		bool IsSuite { get; }
	}
}

