// ***********************************************************************
// Copyright (c) 2010 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Threading;
using System.Globalization;
using NUnit.Framework;
#if !NETCF && !SILVERLIGHT
using System.Security.Principal;
#endif
#if !NUNITLITE
using NUnit.TestData.TestContextData;
using NUnit.TestUtilities;
#endif

namespace NUnit.Framework.Internal
{
	/// <summary>
	/// Summary description for TestExecutionContextTests.
	/// </summary>
	[TestFixture][Property("Question", "Why?")]
	public class TestExecutionContextTests
	{
        TestExecutionContext fixtureContext;
        TestExecutionContext setupContext;

#if !NETCF
        CultureInfo currentCulture;
        CultureInfo currentUICulture;
#endif

#if !NETCF && !SILVERLIGHT
        string currentDirectory;
        IPrincipal currentPrincipal;
#endif

        [TestFixtureSetUp]
        public void OneTimeSetUp()
        {
            fixtureContext = TestExecutionContext.CurrentContext;
        }

        [TestFixtureTearDown]
        public void OneTimeTearDown()
        {
            // TODO: We put some tests in one time teardown to verify that
            // the context is still valid. It would be better if these tests
            // were placed in a second-level test, invoked from this test class.
            TestExecutionContext ec = TestExecutionContext.CurrentContext;
            Assert.That(ec.CurrentTest.Name, Is.EqualTo("TestExecutionContextTests"));
            Assert.That(ec.CurrentTest.FullName,
                Is.EqualTo("NUnit.Framework.Internal.TestExecutionContextTests"));
            Assert.That(fixtureContext.CurrentTest.Id, Is.GreaterThan(0));
            Assert.That(fixtureContext.CurrentTest.Properties.Get("Question"), Is.EqualTo("Why?"));
        }

		/// <summary>
		/// Since we are testing the mechanism that saves and
		/// restores contexts, we save manually here
		/// </summary>
		[SetUp]
		public void Initialize()
		{
            setupContext = new TestExecutionContext(TestExecutionContext.CurrentContext);
#if !NETCF
            currentCulture = CultureInfo.CurrentCulture;
            currentUICulture = CultureInfo.CurrentUICulture;
#endif

#if !NETCF && !SILVERLIGHT
			currentDirectory = Environment.CurrentDirectory;
            currentPrincipal = Thread.CurrentPrincipal;
#endif
        }

		[TearDown]
		public void Cleanup()
		{
#if !NETCF
			Thread.CurrentThread.CurrentCulture = currentCulture;
            Thread.CurrentThread.CurrentUICulture = currentUICulture;
#endif

#if !NETCF && !SILVERLIGHT
			Environment.CurrentDirectory = currentDirectory;
            Thread.CurrentPrincipal = currentPrincipal;
#endif

            Assert.That(
                TestExecutionContext.CurrentContext.CurrentTest.FullName,
                Is.EqualTo(setupContext.CurrentTest.FullName),
                "Context at TearDown failed to match that saved from SetUp");
        }

        [Test]
        public void FixtureSetUpCanAccessFixtureName()
        {
            Assert.That(fixtureContext.CurrentTest.Name, Is.EqualTo("TestExecutionContextTests"));
        }

        [Test]
        public void FixtureSetUpCanAccessFixtureFullName()
        {
            Assert.That(fixtureContext.CurrentTest.FullName,
                Is.EqualTo("NUnit.Framework.Internal.TestExecutionContextTests"));
        }

        [Test]
        public void FixtureSetUpCanAccessFixtureId()
        {
            Assert.That(fixtureContext.CurrentTest.Id, Is.GreaterThan(0));
        }

        [Test]
        public void FixtureSetUpCanAccessFixtureProperties()
        {
            Assert.That(fixtureContext.CurrentTest.Properties.Get("Question"), Is.EqualTo("Why?"));
        }

        [Test]
        public void SetUpCanAccessTestName()
        {
            Assert.That(setupContext.CurrentTest.Name, Is.EqualTo("SetUpCanAccessTestName"));
        }

        [Test]
        public void SetUpCanAccessTestFullName()
        {
            Assert.That(setupContext.CurrentTest.FullName,
                Is.EqualTo("NUnit.Framework.Internal.TestExecutionContextTests.SetUpCanAccessTestFullName"));
        }

        [Test]
        public void SetUpCanAccessTestId()
        {
            Assert.That(setupContext.CurrentTest.Id, Is.GreaterThan(0));
        }

        [Test]
        [Property("Answer", 42)]
        public void SetUpCanAccessTestProperties()
        {
            Assert.That(setupContext.CurrentTest.Properties.Get("Answer"), Is.EqualTo(42));
        }

        [Test]
        public void TestCanAccessItsOwnName()
        {
            Assert.That(TestExecutionContext.CurrentContext.CurrentTest.Name, Is.EqualTo("TestCanAccessItsOwnName"));
        }

        [Test]
        public void TestCanAccessItsOwnFullName()
        {
            Assert.That(TestExecutionContext.CurrentContext.CurrentTest.FullName,
                Is.EqualTo("NUnit.Framework.Internal.TestExecutionContextTests.TestCanAccessItsOwnFullName"));
        }

        [Test]
        public void TestCanAccessItsOwnId()
        {
            Assert.That(TestExecutionContext.CurrentContext.CurrentTest.Id, Is.GreaterThan(0));
        }

        [Test]
        [Property("Answer", 42)]
        public void TestCanAccessItsOwnProperties()
        {
            Assert.That(TestExecutionContext.CurrentContext.CurrentTest.Properties.Get("Answer"), Is.EqualTo(42));
        }

#if !NETCF
        [Test]
        public void SetAndRestoreCurrentCulture()
        {
            Assert.AreEqual(setupContext.CurrentCulture, CultureInfo.CurrentCulture, "Culture not in initial context");

            TestExecutionContext context = setupContext.Save();

            try
            {
                CultureInfo otherCulture =
                    new CultureInfo(currentCulture.Name == "fr-FR" ? "en-GB" : "fr-FR");
                context.CurrentCulture = otherCulture;
                Assert.AreEqual(otherCulture, CultureInfo.CurrentCulture, "Culture was not set");
                Assert.AreEqual(otherCulture, context.CurrentCulture, "Culture not in new context");
            }
            finally
            {
                context = context.Restore();
            }

            Assert.AreEqual(currentCulture, CultureInfo.CurrentCulture, "Culture was not restored");
            Assert.AreEqual(currentCulture, context.CurrentCulture, "Culture not in final context");
        }

        [Test]
        public void SetAndRestoreCurrentUICulture()
        {
            Assert.AreEqual(currentUICulture, setupContext.CurrentUICulture, "UICulture not in initial context");

            TestExecutionContext context = setupContext.Save();

            try
            {
                CultureInfo otherCulture =
                    new CultureInfo(currentUICulture.Name == "fr-FR" ? "en-GB" : "fr-FR");
                context.CurrentUICulture = otherCulture;
                Assert.AreEqual(otherCulture, CultureInfo.CurrentUICulture, "UICulture was not set");
                Assert.AreEqual(otherCulture, context.CurrentUICulture, "UICulture not in new context");
            }
            finally
            {
                context = context.Restore();
            }

            Assert.AreEqual(currentUICulture, CultureInfo.CurrentUICulture, "UICulture was not restored");
            Assert.AreEqual(currentUICulture, context.CurrentUICulture, "UICulture not in final context");
        }
#endif
    }
}
