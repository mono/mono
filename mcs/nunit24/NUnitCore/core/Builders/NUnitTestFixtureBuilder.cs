// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NUnit.Core.Builders
{
	/// <summary>
	/// Built-in SuiteBuilder for NUnit TestFixture
	/// </summary>
	public class NUnitTestFixtureBuilder : AbstractFixtureBuilder
	{
		public NUnitTestFixtureBuilder()
		{
			this.testCaseBuilders.Install( new NUnitTestCaseBuilder() );
		}

		#region AbstractFixtureBuilder Overrides
		/// <summary>
		/// Makes an NUnitTestFixture instance
		/// </summary>
		/// <param name="type">The type to be used</param>
		/// <returns>An NUnitTestFixture as a TestSuite</returns>
		protected override TestSuite MakeSuite( Type type )
		{
			return new NUnitTestFixture( type );
		}

		/// <summary>
		/// Method that sets properties of the test suite based on the
		/// information in the provided Type.
		/// </summary>
		/// <param name="type">The type to examine</param>
		/// <param name="suite">The test suite being constructed</param>
		protected override void SetTestSuiteProperties( Type type, TestSuite suite )
		{
			base.SetTestSuiteProperties( type, suite );

            NUnitFramework.ApplyCommonAttributes( type, suite );
		}

        /// <summary>
        /// Checks to see if the fixture type has the test fixture
        /// attribute type specified in the parameters. Override
        /// to allow additional types - based on name, for example.
        /// </summary>
        /// <param name="type">The fixture type to check</param>
        /// <returns>True if the fixture can be built, false if not</returns>
        public override bool CanBuildFrom(Type type)
        {
            return Reflect.HasAttribute( type, NUnitFramework.TestFixtureAttribute, true );
        }

        /// <summary>
        /// Check that the fixture is valid. In addition to the base class
        /// check for a valid constructor, this method ensures that there 
        /// is no more than one of each setup or teardown method and that
        /// their signatures are correct.
        /// </summary>
        /// <param name="fixtureType">The type of the fixture to check</param>
        /// <param name="reason">A message indicating why the fixture is invalid</param>
        /// <returns>True if the fixture is valid, false if not</returns>
        protected override bool IsValidFixtureType(Type fixtureType, ref string reason)
        {
            if (!base.IsValidFixtureType(fixtureType, ref reason))
                return false;

            if (!fixtureType.IsPublic && !fixtureType.IsNestedPublic)
            {
                reason = "Fixture class is not public";
                return false;
            }

            return CheckSetUpTearDownMethod(fixtureType, "SetUp", NUnitFramework.SetUpAttribute, ref reason)
                && CheckSetUpTearDownMethod(fixtureType, "TearDown", NUnitFramework.TearDownAttribute, ref reason)
                && CheckSetUpTearDownMethod(fixtureType, "TestFixtureSetUp", NUnitFramework.FixtureSetUpAttribute, ref reason)
                && CheckSetUpTearDownMethod(fixtureType, "TestFixtureTearDown", NUnitFramework.FixtureTearDownAttribute, ref reason);
        }

        /// <summary>
        /// Internal helper to check a single setup or teardown method
        /// </summary>
        /// <param name="fixtureType">The type to be checked</param>
        /// <param name="attributeName">The short name of the attribute to be checked</param>
        /// <returns>True if the method is present no more than once and has a valid signature</returns>
        private bool CheckSetUpTearDownMethod(Type fixtureType, string name, string attributeName, ref string reason)
        {
            int count = Reflect.CountMethodsWithAttribute(
                fixtureType, attributeName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly,
                true);

            if (count == 0) return true;

            if (count > 1)
            {
                reason = string.Format("More than one {0} method", name);
                return false;
            }

            MethodInfo theMethod = Reflect.GetMethodWithAttribute(
                fixtureType, attributeName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly,
                true);

            if (theMethod != null)
            {
                if (theMethod.IsStatic ||
                    theMethod.IsAbstract ||
                    !theMethod.IsPublic && !theMethod.IsFamily ||
                    theMethod.GetParameters().Length != 0 ||
                    !theMethod.ReturnType.Equals(typeof(void)))
                {
                    reason = string.Format("Invalid {0} method signature", name);
                    return false;
                }
            }

            return true;
        }
		#endregion
	}
}