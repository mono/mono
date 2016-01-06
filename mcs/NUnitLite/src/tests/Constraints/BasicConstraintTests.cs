// ***********************************************************************
// Copyright (c) 2012 Charlie Poole
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

namespace NUnit.Framework.Constraints.Tests
{
    [TestFixture]
    public class NullConstraintTest : ConstraintTestBase
    {
        [SetUp]
        public void SetUp()
        {
            theConstraint = new NullConstraint();
            stringRepresentation = "<null>";
            expectedDescription = "null";
        }

        internal object[] SuccessData = new object[] { null };

        internal object[] FailureData = new object[] { new object[] { "hello", "\"hello\"" } };
    }

    [TestFixture]
    public class TrueConstraintTest : ConstraintTestBase
    {
        [SetUp]
        public void SetUp()
        {
            theConstraint = new TrueConstraint();
            stringRepresentation = "<true>";
            expectedDescription = "True";
        }

        internal object[] SuccessData = new object[] { true, 2 + 2 == 4 };

        internal object[] FailureData = new object[] { 
            new object[] { null, "null" }, 
            new object[] { "hello", "\"hello\"" },
            new object[] { false, "False" },
            new object[] { 2 + 2 == 5, "False" } };
    }

    [TestFixture]
    public class FalseConstraintTest : ConstraintTestBase
    {
        [SetUp]
        public void SetUp()
        {
            theConstraint = new FalseConstraint();
            stringRepresentation = "<false>";
            expectedDescription = "False";
        }

        internal object[] SuccessData = new object[] { false, 2 + 2 == 5 };

        internal object[] FailureData = new object[] { 
            new object[] { null, "null" },
            new object[] { "hello", "\"hello\"" },
            new object[] { true, "True" },
            new object[] { 2 + 2 == 4, "True" } };
    }

    [TestFixture]
    public class NaNConstraintTest : ConstraintTestBase
    {
        [SetUp]
        public void SetUp()
        {
            theConstraint = new NaNConstraint();
            stringRepresentation = "<nan>";
            expectedDescription = "NaN";
        }

        internal object[] SuccessData = new object[] { double.NaN, float.NaN };

        internal object[] FailureData = new object[] { 
            new object[] { null, "null" },
            new object[] { "hello", "\"hello\"" },
            new object[] { 42, "42" },
            new object[] { double.PositiveInfinity, "Infinity" },
            new object[] { double.NegativeInfinity, "-Infinity" },
            new object[] { float.PositiveInfinity, "Infinity" },
            new object[] { float.NegativeInfinity, "-Infinity" } };
    }
}
