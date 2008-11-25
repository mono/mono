// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Diagnostics;

namespace NUnit.Core.Builders
{
	public class NUnitTestCaseBuilder : AbstractTestCaseBuilder
	{
		private bool allowOldStyleTests = NUnitFramework.AllowOldStyleTests;

        #region AbstractTestCaseBuilder Overrides
		/// <summary>
		/// Determine if the method is an NUnit test method.
		/// The method must normally be marked with the test
		/// attribute for this to be true. If the test config
		/// file sets AllowOldStyleTests to true, then any 
		/// method beginning "test..." (case-insensitive)
		/// is treated as a test unless it is also marked
		/// as a setup or teardown method.
		/// </summary>
		/// <param name="method">A MethodInfo for the method being used as a test method</param>
		/// <returns>True if the builder can create a test case from this method</returns>
        public override bool CanBuildFrom(MethodInfo method)
        {
            if ( Reflect.HasAttribute( method, NUnitFramework.TestAttribute, false ) )
                return true;

            if (allowOldStyleTests)
            {
                Regex regex = new Regex("^(?i:test)");
                if ( regex.Match(method.Name).Success 
					&& !NUnitFramework.IsSetUpMethod( method )
					&& !NUnitFramework.IsTearDownMethod( method )
					&& !NUnitFramework.IsFixtureSetUpMethod( method )
					&& !NUnitFramework.IsFixtureTearDownMethod( method ) )
						return true;
            }

            return false;
        }

		/// <summary>
		/// Create an NUnitTestMethod
		/// </summary>
		/// <param name="method">A MethodInfo for the method being used as a test method</param>
		/// <returns>A new NUnitTestMethod</returns>
        protected override TestCase MakeTestCase(MethodInfo method)
        {
			return new NUnitTestMethod( method );
        }

		/// <summary>
		/// Set additional properties of the newly created test case based
		/// on its attributes. As implemented, the method sets the test's
		/// RunState,  Description, Categories and Properties.
		/// </summary>
		/// <param name="method">A MethodInfo for the method being used as a test method</param>
		/// <param name="testCase">The test case being constructed</param>
		protected override void SetTestProperties( MethodInfo method, TestCase testCase )
		{
            NUnitFramework.ApplyCommonAttributes( method, testCase );
			NUnitFramework.ApplyExpectedExceptionAttribute( method, (TestMethod)testCase );
		}
		#endregion
    }
}
