// ****************************************************************
// Copyright 2008, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org
// ****************************************************************

using System;
using System.Threading;
using System.Collections;

namespace NUnit.Framework.Syntax
{
	public class AfterTest_SimpleConstraint : SyntaxTest
	{
		[SetUp]
		public void SetUp()
		{
			parseTree = "<after 1000 <equal 10>>";
			staticSyntax = Is.EqualTo(10).After(1000);
			inheritedSyntax = Helper().EqualTo(10).After(1000);
			builderSyntax = Builder().EqualTo(10).After(1000);
		}
	}

	public class AfterTest_ProperyTest : SyntaxTest
	{
		[SetUp]
		public void SetUp()
		{
			parseTree = "<after 1000 <property X <equal 10>>>";
			staticSyntax = Has.Property("X").EqualTo(10).After(1000);
			inheritedSyntax = Helper().Property("X").EqualTo(10).After(1000);
			builderSyntax = Builder().Property("X").EqualTo(10).After(1000);
		}
	}

	public class AfterTest_AndOperator : SyntaxTest
	{
		[SetUp]
		public void SetUp()
		{
			parseTree = "<after 1000 <and <greaterthan 0> <lessthan 10>>>";
			staticSyntax = Is.GreaterThan(0).And.LessThan(10).After(1000);
			inheritedSyntax = Helper().GreaterThan(0).And.LessThan(10).After(1000);
			builderSyntax = Builder().GreaterThan(0).And.LessThan(10).After(1000);
		}
    }

#if CLR_2_0 || CLR_4_0
    public abstract class AfterSyntaxTests
    {
        protected bool flag;
        protected int num;
        protected object ob1, ob2, ob3;
        protected ArrayList list;
        protected string greeting;

        [SetUp]
        public void InitializeValues()
        {
            this.flag = false;
            this.num = 0;
            this.ob1 = new object();
            this.ob2 = new object();
            this.ob3 = new object();
            this.list = new ArrayList();
            this.list.Add(1);
            this.list.Add(2);
            this.list.Add(3);
            this.greeting = "hello";

            new Thread(ModifyValuesAfterDelay).Start();
        }

        private void ModifyValuesAfterDelay()
        {
            Thread.Sleep(100);

            this.flag = true;
            this.num = 1;
            this.ob1 = ob2;
            this.ob3 = null;
            this.list.Add(4);
            this.greeting += "world";
        }
    }

#if !NETCF_2_0
    // This compiles under VS2008 but not using NAnt
    // TODO: Make Nant script use the highest level of msbuild available
    public class AfterSyntaxUsingAnonymousDelegates : AfterSyntaxTests
    {
        [Test]
        public void TrueTest()
        {
            Assert.That(delegate { return flag; }, Is.True.After(5000, 200));
        }

        [Test]
        public void EqualToTest()
        {
            Assert.That(delegate { return num; }, Is.EqualTo(1).After(5000, 200));
        }

        [Test]
        public void SameAsTest()
        {
            Assert.That(delegate { return ob1; }, Is.SameAs(ob2).After(5000, 200));
        }

        [Test]
        public void GreaterTest()
        {
            Assert.That(delegate { return num; }, Is.GreaterThan(0).After(5000,200));
        }

        [Test]
        public void HasMemberTest()
        {
            Assert.That(delegate { return list; }, Has.Member(4).After(5000, 200));
        }

        [Test]
        public void NullTest()
        {
            Assert.That(delegate { return ob3; }, Is.Null.After(5000, 200));
        }

        [Test]
        public void TextTest()
        {
            Assert.That(delegate { return greeting; }, Is.StringEnding("world").After(5000, 200));
        }

		[Test]
		public void ThrowsTest()
		{
			Assert.That(delegate { throw new Exception(); }, Throws.TypeOf<Exception>().After(100));
		}
    }
#endif

    public class AfterSyntaxUsingActualPassedByRef : AfterSyntaxTests
    {
        [Test]
        public void TrueTest()
        {
            Assert.That(ref flag, Is.True.After(5000, 200));
        }

        [Test]
        public void EqualToTest()
        {
            Assert.That(ref num, Is.EqualTo(1).After(5000, 200));
        }

        [Test]
        public void SameAsTest()
        {
            Assert.That(ref ob1, Is.SameAs(ob2).After(5000, 200));
        }

        [Test]
        public void GreaterTest()
        {
            Assert.That(ref num, Is.GreaterThan(0).After(5000, 200));
        }

        [Test]
        public void HasMemberTest()
        {
            Assert.That(ref list, Has.Member(4).After(5000, 200));
        }

        [Test]
        public void NullTest()
        {
            Assert.That(ref ob3, Is.Null.After(5000, 200));
        }

        [Test]
        public void TextTest()
        {
            Assert.That(ref greeting, Is.StringEnding("world").After(5000, 200));
        }
    }
#endif
}