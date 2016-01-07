// ***********************************************************************
// Copyright (c) 2009 Charlie Poole
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

namespace NUnit.Framework.Syntax
{
    #region Not
    public class NotTest : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<not <null>>";
            staticSyntax = Is.Not.Null;
            inheritedSyntax = Helper().Not.Null;
            builderSyntax = Builder().Not.Null;
        }
    }

    public class NotTest_Cascaded : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<not <not <not <null>>>>";
            staticSyntax = Is.Not.Not.Not.Null;
            inheritedSyntax = Helper().Not.Not.Not.Null;
            builderSyntax = Builder().Not.Not.Not.Null;
        }
    }
    #endregion

    #region All
    public class AllTest : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<all <greaterthan 0>>";
            staticSyntax = Is.All.GreaterThan(0);
            inheritedSyntax = Helper().All.GreaterThan(0);
            builderSyntax = Builder().All.GreaterThan(0);
        }
    }
    #endregion

    #region Some
    public class SomeTest : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<some <equal 3>>";
            staticSyntax = Has.Some.EqualTo(3);
            inheritedSyntax = Helper().Some.EqualTo(3);
            builderSyntax = Builder().Some.EqualTo(3);
        }
    }

    public class SomeTest_BeforeBinaryOperators : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<some <or <and <greaterthan 0> <lessthan 100>> <equal 999>>>";
            staticSyntax = Has.Some.GreaterThan(0).And.LessThan(100).Or.EqualTo(999);
            inheritedSyntax = Helper().Some.GreaterThan(0).And.LessThan(100).Or.EqualTo(999);
            builderSyntax = Builder().Some.GreaterThan(0).And.LessThan(100).Or.EqualTo(999);
        }
    }

    public class SomeTest_NestedSome : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<some <some <lessthan 100>>>";
            staticSyntax = Has.Some.With.Some.LessThan(100);
            inheritedSyntax = Helper().Some.With.Some.LessThan(100);
            builderSyntax = Builder().Some.With.Some.LessThan(100);
        }
        
    }

    public class SomeTest_UseOfAndSome : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<and <some <greaterthan 0>> <some <lessthan 100>>>";
            staticSyntax = Has.Some.GreaterThan(0).And.Some.LessThan(100);
            inheritedSyntax = Helper().Some.GreaterThan(0).And.Some.LessThan(100);
            builderSyntax = Builder().Some.GreaterThan(0).And.Some.LessThan(100);
        }
    }
    #endregion

    #region None
    public class NoneTest : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<none <lessthan 0>>";
            staticSyntax = Has.None.LessThan(0);
            inheritedSyntax = Helper().None.LessThan(0);
            builderSyntax = Builder().None.LessThan(0);
        }
    }
    #endregion

    #region And
    public class AndTest : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<and <greaterthan 5> <lessthan 10>>";
            staticSyntax = Is.GreaterThan(5).And.LessThan(10);
            inheritedSyntax = Helper().GreaterThan(5).And.LessThan(10);
            builderSyntax = Builder().GreaterThan(5).And.LessThan(10);
        }
    }

    public class AndTest_ThreeAndsWithNot : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<and <not <null>> <and <not <lessthan 5>> <not <greaterthan 10>>>>";
            staticSyntax = Is.Not.Null.And.Not.LessThan(5).And.Not.GreaterThan(10);
            inheritedSyntax = Helper().Not.Null.And.Not.LessThan(5).And.Not.GreaterThan(10);
            builderSyntax = Builder().Not.Null.And.Not.LessThan(5).And.Not.GreaterThan(10);
        }
    }
    #endregion

    #region Or
    public class OrTest : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<or <lessthan 5> <greaterthan 10>>";
            staticSyntax = Is.LessThan(5).Or.GreaterThan(10);
            inheritedSyntax = Helper().LessThan(5).Or.GreaterThan(10);
            builderSyntax = Builder().LessThan(5).Or.GreaterThan(10);
        }
    }

    public class OrTest_ThreeOrs : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<or <lessthan 5> <or <greaterthan 10> <equal 7>>>";
            staticSyntax = Is.LessThan(5).Or.GreaterThan(10).Or.EqualTo(7);
            inheritedSyntax = Helper().LessThan(5).Or.GreaterThan(10).Or.EqualTo(7);
            builderSyntax = Builder().LessThan(5).Or.GreaterThan(10).Or.EqualTo(7);
        }
    }
    #endregion

    #region Binary Operator Precedence
    public class AndIsEvaluatedBeforeFollowingOr : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<or <and <lessthan 100> <greaterthan 0>> <equal 999>>";
            staticSyntax = Is.LessThan(100).And.GreaterThan(0).Or.EqualTo(999);
            inheritedSyntax = Helper().LessThan(100).And.GreaterThan(0).Or.EqualTo(999);
            builderSyntax = Builder().LessThan(100).And.GreaterThan(0).Or.EqualTo(999);
        }
    }

    public class AndIsEvaluatedBeforePrecedingOr : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<or <equal 999> <and <greaterthan 0> <lessthan 100>>>";
            staticSyntax = Is.EqualTo(999).Or.GreaterThan(0).And.LessThan(100);
            inheritedSyntax = Helper().EqualTo(999).Or.GreaterThan(0).And.LessThan(100);
            builderSyntax = Builder().EqualTo(999).Or.GreaterThan(0).And.LessThan(100);
        }
    }
    #endregion

    public class OperatorPrecedenceTests
    {
        public class A
        {
            public B B
            {
                get { return new B(); }
            }

            public string X
            {
                get { return "X in A"; }
            }

            public string Y
            {
                get { return "Y in A"; }
            }
        }

        public class B
        {
            public string X
            {
                get { return "X in B"; }
            }

            public string Y
            {
                get { return "Y in B"; }
            }
        }

        [Test]
        public void WithTests()
        {
            A a = new A();
            Assert.That(a, Has.Property("X").EqualTo("X in A")
                          .And.Property("Y").EqualTo("Y in A"));
            Assert.That(a, Has.Property("X").EqualTo("X in A")
                          .And.Property("B").Property("X").EqualTo("X in B"));
            Assert.That(a, Has.Property("X").EqualTo("X in A")
                          .And.Property("B").With.Property("X").EqualTo("X in B"));
            Assert.That(a, Has.Property("B").Property("X").EqualTo("X in B")
                          .And.Property("B").Property("Y").EqualTo("Y in B"));
            Assert.That(a, Has.Property("B").Property("X").EqualTo("X in B")
                          .And.Property("B").With.Property("Y").EqualTo("Y in B"));
            Assert.That(a, Has.Property("B").With.Property("X").EqualTo("X in B")
                                            .And.Property("Y").EqualTo("Y in B"));
        }

        [Test]
        public void SomeTests()
        {
            string[] array = new string[] { "a", "aa", "x", "xy", "xyz" };
            //Assert.That(array, Has.Some.StartsWith("a").And.Some.Length.EqualTo(3));
            Assert.That(array, Has.None.StartsWith("a").And.Length.EqualTo(3));
            Assert.That(array, Has.Some.StartsWith("x").And.Length.EqualTo(3));
        }
    }
}
