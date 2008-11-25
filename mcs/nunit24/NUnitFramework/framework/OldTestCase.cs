// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Framework
{
	using System;

	/// <summary>
	/// Obsolete class, formerly used to identify tests through
	/// inheritance. Avoid using this class for new tests.
	/// </summary>
	[TestFixture]
	[Obsolete("use TestFixture attribute instead of inheritance",false)]
	public class TestCase : Assertion
	{
		/// <summary>
		/// Method called immediately before running the test.
		/// </summary>
		[SetUp]
		[Obsolete("use SetUp attribute instead of naming convention",false)]
		protected virtual void SetUp()
		{}

		/// <summary>
		/// Method Called immediately after running the test. It is
		/// guaranteed to be called, even if an exception is thrown. 
		/// </summary>
		[TearDown]
		[Obsolete("use TearDown attribute instead of naming convention",false)]
		protected virtual void TearDown()
		{}
	}
}
