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

namespace NUnit.TestData.SetUpData
{
	[TestFixture]
	public class SetUpAndTearDownFixture
	{
        public bool wasSetUpCalled;
        public bool wasTearDownCalled;

        [SetUp]
        public virtual void Init()
        {
            wasSetUpCalled = true;
        }

        [TearDown]
        public virtual void Destroy()
        {
            wasTearDownCalled = true;
        }

        [Test]
        public void Success() { }
    }


	[TestFixture]
	public class SetUpAndTearDownCounterFixture
	{
		public int setUpCounter;
		public int tearDownCounter;

		[SetUp]
		public virtual void Init()
		{
			setUpCounter++;
		}

		[TearDown]
		public virtual void Destroy()
		{
			tearDownCounter++;
		}

		[Test]
		public void TestOne(){}

		[Test]
		public void TestTwo(){}

		[Test]
		public void TestThree(){}
	}
		
	[TestFixture]
	public class InheritSetUpAndTearDown : SetUpAndTearDownFixture
	{
		[Test]
		public void AnotherTest(){}
	}

	[TestFixture]
	public class DefineInheritSetUpAndTearDown : SetUpAndTearDownFixture
	{
		public bool derivedSetUpCalled;
		public bool derivedTearDownCalled;

		[SetUp]
		public override void Init()
		{
			derivedSetUpCalled = true;
		}

		[TearDown]
		public override void Destroy()
		{
			derivedTearDownCalled = true;
		}

		[Test]
		public void AnotherTest(){}
	}

    public class MultipleSetUpTearDownFixture
    {
        public bool wasSetUp1Called;
        public bool wasSetUp2Called;
        public bool wasSetUp3Called;
        public bool wasTearDown1Called;
        public bool wasTearDown2Called;

        [SetUp]
        public virtual void Init1()
        {
            wasSetUp1Called = true;
        }
        [SetUp]
        public virtual void Init2()
        {
            wasSetUp2Called = true;
        }
        [SetUp]
        public virtual void Init3()
        {
            wasSetUp3Called = true;
        }

        [TearDown]
        public virtual void TearDown1()
        {
            wasTearDown1Called = true;
        }
        [TearDown]
        public virtual void TearDown2()
        {
            wasTearDown2Called = true;
        }

        [Test]
        public void Success() { }
    }

    [TestFixture]
    public class DerivedClassWithSeparateSetUp : SetUpAndTearDownFixture
    {
        public bool wasDerivedSetUpCalled;
        public bool wasDerivedTearDownCalled;
        public bool wasBaseSetUpCalledFirst;
        public bool wasBaseTearDownCalledLast;

        [SetUp]
        public void DerivedInit()
        {
            wasDerivedSetUpCalled = true;
            wasBaseSetUpCalledFirst = wasSetUpCalled;
        }

        [TearDown]
        public void DerivedTearDown()
        {
            wasDerivedTearDownCalled = true;
            wasBaseTearDownCalledLast = !wasTearDownCalled;
        }
    }

    [TestFixture]
    public class SetupAndTearDownExceptionFixture
    {
        public Exception setupException;
        public Exception tearDownException;

        [SetUp] 
        public void SetUp()
        {
            if (setupException != null) throw setupException;
        }

        [TearDown]
        public void TearDown()
        {
            if (tearDownException!=null) throw tearDownException;
        }

        [Test]
        public void TestOne() {}
    }
}
