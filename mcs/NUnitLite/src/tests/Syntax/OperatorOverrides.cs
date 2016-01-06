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
using NUnit.Framework.Constraints;

namespace NUnit.Framework.Syntax
{
    public class NotOperatorOverride : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<not <null>>";
            staticSyntax = !Is.Null;
            inheritedSyntax = !Helper().Null;
            builderSyntax = !Builder().Null;
        }

        [Test]
        public void NotOperatorCanApplyToResolvableConstraintExpression()
        {
            Assert.That(GetType(), !Has.Attribute(typeof(DescriptionAttribute)));
        }
    }

    [TestFixture, Description("Test")]
    public class AndOperatorOverride : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<and <greaterthan 5> <lessthan 10>>";
            staticSyntax = Is.GreaterThan(5) & Is.LessThan(10);
            inheritedSyntax = Helper().GreaterThan(5) & Is.LessThan(10);
            builderSyntax = Builder().GreaterThan(5) & Builder().LessThan(10);
        }

        [Test]
        public void AndOperatorCanCombineTwoResolvableConstraintExpressions()
        {
            Assert.That(GetType(), Has.Attribute(typeof(TestFixtureAttribute)) & Has.Attribute(typeof(DescriptionAttribute)));
        }

        [Test]
        public void AndOperatorCanCombineConstraintAndResolvableConstraintExpression()
        {
            Assert.That(GetType(), Is.EqualTo(typeof(AndOperatorOverride)) & Has.Attribute(typeof(DescriptionAttribute)));
        }

        [Test]
        public void AndOperatorCanCombineResolvableConstraintExpressionAndConstraint()
        {
            Assert.That(GetType(), Has.Attribute(typeof(DescriptionAttribute)) & Is.EqualTo(typeof(AndOperatorOverride)));
        }
    }

    [TestFixture]
    public class OrOperatorOverride : SyntaxTest
    {
        [SetUp]
        public void SetUp()
        {
            parseTree = "<or <lessthan 5> <greaterthan 10>>";
            staticSyntax = Is.LessThan(5) | Is.GreaterThan(10);
            inheritedSyntax = Helper().LessThan(5) | Is.GreaterThan(10);
            builderSyntax = Builder().LessThan(5) | Is.GreaterThan(10);
        }

        [Test]
        public void OrOperatorCanCombineTwoResolvableConstraintExpressions()
        {
            Assert.That(GetType(), Has.Attribute(typeof(TestFixtureAttribute)) | Has.Attribute(typeof(TestCaseAttribute)));
        }

        [Test]
        public void OrOperatorCanCombineResolvableConstraintExpressionAndConstraint()
        {
            Assert.That(GetType(), Has.Attribute(typeof(TestFixtureAttribute)) | Is.EqualTo(7));
        }

        [Test]
        public void OrOperatorCanCombineConstraintAndResolvableConstraintExpression()
        {
            Assert.That(GetType(), Is.EqualTo(7) | Has.Attribute(typeof(TestFixtureAttribute)));
        }
    }

    public class MixedOperatorOverrides
    {
        [Test]
        public void ComplexTests()
        {
            string expected = "<and <and <not <null>> <not <lessthan 5>>> <not <greaterthan 10>>>";

            Constraint c =
                Is.Not.Null & Is.Not.LessThan(5) & Is.Not.GreaterThan(10);
            Assert.That(c.ToString(), Is.EqualTo(expected).NoClip);

            c = !Is.Null & !Is.LessThan(5) & !Is.GreaterThan(10);
            Assert.That(c.ToString(), Is.EqualTo(expected).NoClip);

            Constraint x = null;
            c = !x & !Is.LessThan(5) & !Is.GreaterThan(10);
            Assert.That(c.ToString(), Is.EqualTo(expected).NoClip);
        }
    }
}
