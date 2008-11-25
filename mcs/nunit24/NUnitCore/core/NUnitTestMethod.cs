// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;
using System.Reflection;

namespace NUnit.Core
{
	/// <summary>
	/// Class to implement an NUnit test method
	/// </summary>
	public class NUnitTestMethod : TestMethod
	{
		#region Constructor
		public NUnitTestMethod(MethodInfo method) : base(method) 
        {
            this.setUpMethod = NUnitFramework.GetSetUpMethod(this.FixtureType);
            this.tearDownMethod = NUnitFramework.GetTearDownMethod(this.FixtureType);
        }
		#endregion

		#region TestMethod Overrides
		/// <summary>
		/// Run a test returning the result. Overrides TestMethod
		/// to count assertions.
		/// </summary>
		/// <param name="testResult"></param>
		public override void Run(TestCaseResult testResult)
		{
			base.Run(testResult);

			testResult.AssertCount = NUnitFramework.GetAssertCount();
		}

		/// <summary>
		/// Determine if an exception is an NUnit AssertionException
		/// </summary>
		/// <param name="ex">The exception to be examined</param>
		/// <returns>True if it's an NUnit AssertionException</returns>
		protected override bool IsAssertException(Exception ex)
		{
            return ex.GetType().FullName == NUnitFramework.AssertException;
		}

		/// <summary>
		/// Determine if an exception is an NUnit IgnoreException
		/// </summary>
		/// <param name="ex">The exception to be examined</param>
		/// <returns>True if it's an NUnit IgnoreException</returns>
		protected override bool IsIgnoreException(Exception ex)
		{
            return ex.GetType().FullName == NUnitFramework.IgnoreException;
		}
		#endregion
	}
}
