// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
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
using NUnit.Framework;
#if !NETCF
using System.Security.Principal;
#endif

namespace NUnit.TestData.TestFixtureData
{
	/// <summary>
	/// Classes used for testing NUnit
	/// </summary>

    [TestFixture]
    public class NoDefaultCtorFixture
    {
        public NoDefaultCtorFixture(int index) { }

        [Test]
        public void OneTest() { }
    }

    [TestFixture(7,3)]
    public class FixtureWithArgsSupplied
    {
        public FixtureWithArgsSupplied(int x, int y) { }

        [Test]
        public void OneTest() { }
    }

    [TestFixture]
	public class BadCtorFixture
	{
		BadCtorFixture()
		{
			throw new Exception();
		}

		[Test] public void OneTest()
		{}
	}

    [TestFixture]
    public class FixtureWithTestFixtureAttribute
    {
        [Test]
        public void SomeTest() { }
    }

    public class FixtureWithoutTestFixtureAttributeContainingTest
    {
        [Test]
        public void SomeTest() { }
    }

    public class FixtureWithoutTestFixtureAttributeContainingTestCase
    {
        [TestCase(42)]
        public void SomeTest(int x) { }
    }
 
    public class FixtureWithoutTestFixtureAttributeContainingTestCaseSource
    {
        [TestCaseSource("data")]
        public void SomeTest(int x) { }
    }
 
#if !NUNITLITE
    public class FixtureWithoutTestFixtureAttributeContainingTheory
    {
        [Theory]
        public void SomeTest(int x) { }
    }
#endif
 
#if CLR_2_0 || CLR_4_0
    public static class StaticFixtureWithoutTestFixtureAttribute
    {
        [Test]
        public static void StaticTest() { }
    }
#endif

    [TestFixture]
	public class MultipleSetUpAttributes
	{
		[SetUp]
		public void Init1()
		{}

		[SetUp]
		public void Init2()
		{}

		[Test] public void OneTest()
		{}
	}

	[TestFixture]
	public class MultipleTearDownAttributes
	{
		[TearDown]
		public void Destroy1()
		{}

		[TearDown]
		public void Destroy2()
		{}

		[Test] public void OneTest()
		{}
	}

	[TestFixture]
	[Ignore("testing ignore a fixture")]
	public class IgnoredFixture
	{
		[Test]
		public void Success()
		{}
	}

	[TestFixture]
	public class OuterClass
	{
		[TestFixture]
		public class NestedTestFixture
		{
			[TestFixture]
				public class DoublyNestedTestFixture
			{
				[Test]
				public void Test()
				{
				}
			}
		}
	}

	[TestFixture]
	public abstract class AbstractTestFixture
	{
		[TearDown]
		public void Destroy1()
		{}

        [Test]
        public void SomeTest()
        {
        }
	}

    public class DerivedFromAbstractTestFixture : AbstractTestFixture
    {
    }

	[TestFixture]
	public class BaseClassTestFixture
	{
		[Test]
		public void Success() { }
	}
	
	public abstract class AbstractDerivedTestFixture : BaseClassTestFixture
	{
        [Test]
        public void Test()
        {
        }
	}

    public class DerivedFromAbstractDerivedTestFixture : AbstractDerivedTestFixture
    {
    }

    [TestFixture]
    public abstract class AbstractBaseFixtureWithAttribute
    {
    }

    [TestFixture]
    public abstract class AbstractDerivedFixtureWithSecondAttribute
        : AbstractBaseFixtureWithAttribute
    {
    }

    public class DoubleDerivedClassWithTwoInheritedAttributes
        : AbstractDerivedFixtureWithSecondAttribute
    {
    }

    [TestFixture]
	public class MultipleFixtureSetUpAttributes
	{
		[TestFixtureSetUp]
		public void Init1()
		{}

		[TestFixtureSetUp]
		public void Init2()
		{}

		[Test] public void OneTest()
		{}
	}

	[TestFixture]
	public class MultipleFixtureTearDownAttributes
	{
		[TestFixtureTearDown]
		public void Destroy1()
		{}

		[TestFixtureTearDown]
		public void Destroy2()
		{}

		[Test] public void OneTest()
		{}
	}

	// Base class used to ensure following classes
	// all have at least one test
	public class OneTestBase
	{
		[Test] public void OneTest() { }
	}

	[TestFixture]
	public class PrivateSetUp : OneTestBase
	{
		[SetUp]
		private void Setup()	{}
	}

	[TestFixture]
	public class ProtectedSetUp : OneTestBase
	{
		[SetUp]
		protected void Setup()	{}
	}

	[TestFixture]
	public class StaticSetUp : OneTestBase
	{
		[SetUp]
		public static void Setup() {}
	}

	[TestFixture]
	public class SetUpWithReturnValue : OneTestBase
	{
		[SetUp]
		public int Setup() { return 0; }
	}

	[TestFixture]
	public class SetUpWithParameters : OneTestBase
	{
		[SetUp]
		public void Setup(int j) { }
	}

	[TestFixture]
	public class PrivateTearDown : OneTestBase
	{
		[TearDown]
		private void Teardown()	{}
	}

	[TestFixture]
	public class ProtectedTearDown : OneTestBase
	{
		[TearDown]
		protected void Teardown()	{}
	}

	[TestFixture]
	public class StaticTearDown : OneTestBase
	{
		[SetUp]
		public static void TearDown() {}
	}

	[TestFixture]
	public class TearDownWithReturnValue : OneTestBase
	{
		[TearDown]
		public int Teardown() { return 0; }
	}

	[TestFixture]
	public class TearDownWithParameters : OneTestBase
	{
		[TearDown]
		public void Teardown(int j) { }
	}

	[TestFixture]
	public class PrivateFixtureSetUp : OneTestBase
	{
		[TestFixtureSetUp]
		private void Setup()	{}
	}

	[TestFixture]
	public class ProtectedFixtureSetUp : OneTestBase
	{
		[TestFixtureSetUp]
		protected void Setup()	{}
	}

	[TestFixture]
	public class StaticFixtureSetUp : OneTestBase
	{
		[TestFixtureSetUp]
		public static void Setup() {}
	}

	[TestFixture]
	public class FixtureSetUpWithReturnValue : OneTestBase
	{
		[TestFixtureSetUp]
		public int Setup() { return 0; }
	}

	[TestFixture]
	public class FixtureSetUpWithParameters : OneTestBase
	{
		[SetUp]
		public void Setup(int j) { }
	}

	[TestFixture]
	public class PrivateFixtureTearDown : OneTestBase
	{
		[TestFixtureTearDown]
		private void Teardown()	{}
	}

	[TestFixture]
	public class ProtectedFixtureTearDown : OneTestBase
	{
		[TestFixtureTearDown]
		protected void Teardown()	{}
	}

	[TestFixture]
	public class StaticFixtureTearDown : OneTestBase
	{
		[TestFixtureTearDown]
		public static void Teardown() {}
	}

	[TestFixture]
	public class FixtureTearDownWithReturnValue : OneTestBase
	{
		[TestFixtureTearDown]
		public int Teardown() { return 0; }
	}

	[TestFixture]
	public class FixtureTearDownWithParameters : OneTestBase
	{
		[TestFixtureTearDown]
		public void Teardown(int j) { }
	}

#if !NETCF && !SILVERLIGHT
    [TestFixture]
    public class FixtureThatChangesTheCurrentPrincipal
    {
        [Test]
        public void ChangeCurrentPrincipal()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            GenericPrincipal principal = new GenericPrincipal( identity, new string[] { } );
            System.Threading.Thread.CurrentPrincipal = principal;
        }
    }
#endif

#if CLR_2_0 || CLR_4_0
#if !NETCF
    [TestFixture(typeof(int))]
    [TestFixture(typeof(string))]
    public class GenericFixtureWithProperArgsProvided<T>
    {
        [Test]
        public void SomeTest() { }
    }

    public class GenericFixtureWithNoTestFixtureAttribute<T>
    {
        [Test]
        public void SomeTest() { }
    }

    [TestFixture]
    public class GenericFixtureWithNoArgsProvided<T>
    {
        [Test]
        public void SomeTest() { }
    }

    [TestFixture]
    public abstract class AbstractFixtureBase
    {
        [Test]
        public void SomeTest() { }
    }

    public class GenericFixtureDerivedFromAbstractFixtureWithNoArgsProvided<T> : AbstractFixtureBase
    {
    }

    [TestFixture(typeof(int))]
    [TestFixture(typeof(string))]
    public class GenericFixtureDerivedFromAbstractFixtureWithArgsProvided<T> : AbstractFixtureBase
    {
    }
#endif
#endif
}
