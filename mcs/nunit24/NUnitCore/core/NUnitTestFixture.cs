// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.Reflection;

namespace NUnit.Core
{
    /// <summary>
    /// Class to implement an NUnit test fixture
    /// </summary>
    public class NUnitTestFixture : TestFixture
    {
        public NUnitTestFixture(Type fixtureType) : base( fixtureType)
        {
            this.fixtureSetUp = NUnitFramework.GetFixtureSetUpMethod( fixtureType );
            this.fixtureTearDown = NUnitFramework.GetFixtureTearDownMethod( fixtureType );
        }

        protected override void DoOneTimeSetUp(TestResult suiteResult)
        {
            base.DoOneTimeSetUp(suiteResult);

			suiteResult.AssertCount = NUnitFramework.GetAssertCount(); ;
        }

        protected override void DoOneTimeTearDown(TestResult suiteResult)
        {
            base.DoOneTimeTearDown(suiteResult);

			suiteResult.AssertCount += NUnitFramework.GetAssertCount();
        }
    }
}
