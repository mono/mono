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
    [TestFixture]
    public class ExactTypeTest : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<typeof System.String>";
            staticSyntax = Is.TypeOf(typeof(string));
            inheritedSyntax = Helper().TypeOf(typeof(string));
            builderSyntax = Builder().TypeOf(typeof(string));
        }
    }

    [TestFixture]
    public class InstanceOfTest : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<instanceof System.String>";
            staticSyntax = Is.InstanceOf(typeof(string));
            inheritedSyntax = Helper().InstanceOf(typeof(string));
            builderSyntax = Builder().InstanceOf(typeof(string));
        }
    }

    [TestFixture]
    public class AssignableFromTest : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<assignablefrom System.String>";
            staticSyntax = Is.AssignableFrom(typeof(string));
            inheritedSyntax = Helper().AssignableFrom(typeof(string));
            builderSyntax = Builder().AssignableFrom(typeof(string));
        }
    }

    [TestFixture]
    public class AssignableToTest : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<assignableto System.String>";
            staticSyntax = Is.AssignableTo(typeof(string));
            inheritedSyntax = Helper().AssignableTo(typeof(string));
            builderSyntax = Builder().AssignableTo(typeof(string));
        }
    }

    [TestFixture]
    public class AttributeTest : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<attributeexists NUnit.Framework.TestFixtureAttribute>";
            staticSyntax = Has.Attribute(typeof(TestFixtureAttribute));
            inheritedSyntax = Helper().Attribute(typeof(TestFixtureAttribute));
            builderSyntax = Builder().Attribute(typeof(TestFixtureAttribute));
        }
    }

    [TestFixture]
    public class AttributeTestWithFollowingConstraint : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = @"<attribute NUnit.Framework.TestFixtureAttribute <property Description <not <null>>>>";
            staticSyntax = Has.Attribute(typeof(TestFixtureAttribute)).Property("Description").Not.Null;
            inheritedSyntax = Helper().Attribute(typeof(TestFixtureAttribute)).Property("Description").Not.Null;
            builderSyntax = Builder().Attribute(typeof(TestFixtureAttribute)).Property("Description").Not.Null;
        }
    }

#if CLR_2_0 || CLR_4_0
    [TestFixture]
    public class ExactTypeTest_Generic : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<typeof System.String>";
            staticSyntax = Is.TypeOf<string>();
            inheritedSyntax = Helper().TypeOf<string>();
            builderSyntax = Builder().TypeOf<string>();
        }
    }

    [TestFixture]
    public class InstanceOfTest_Generic : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<instanceof System.String>";
            staticSyntax = Is.InstanceOf<string>();
            inheritedSyntax = Helper().InstanceOf<string>();
            builderSyntax = Builder().InstanceOf<string>();
        }
    }

    [TestFixture]
    public class AssignableFromTest_Generic : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<assignablefrom System.String>";
            staticSyntax = Is.AssignableFrom<string>();
            inheritedSyntax = Helper().AssignableFrom<string>();
            builderSyntax = Builder().AssignableFrom<string>();
        }
    }

    [TestFixture]
    public class AssignableToTest_Generic : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<assignableto System.String>";
            staticSyntax = Is.AssignableTo<string>();
            inheritedSyntax = Helper().AssignableTo<string>();
            builderSyntax = Builder().AssignableTo<string>();
        }
    }

    [TestFixture]
    public class AttributeTest_Generic : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<attributeexists NUnit.Framework.TestFixtureAttribute>";
            staticSyntax = Has.Attribute<TestFixtureAttribute>();
            inheritedSyntax = Helper().Attribute<TestFixtureAttribute>();
            builderSyntax = Builder().Attribute<TestFixtureAttribute>();
        }
    }
#endif
}
