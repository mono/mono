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

#if (CLR_2_0 || CLR_4_0) && !NETCF_2_0
using System;

namespace NUnit.Framework.Constraints.Tests
{
    [TestFixture]
    public class PredicateConstraintTests : ConstraintTestBase
    {
        [SetUp]
        public void SetUp()
        {
            theConstraint = new PredicateConstraint<int>((x) => x < 5 );
            expectedDescription = @"value matching lambda expression";
            stringRepresentation = "<predicate>";
        }

        internal object[] SuccessData = new object[] 
        {
            0,
            -5
        };

        internal object[] FailureData = new object[]
        {
            new TestCaseData(123, "123")
        };

        [Test]
        public void CanUseConstraintExpressionSyntax()
        {
            Assert.That(123, Is.TypeOf<int>().And.Matches<int>((int x) => x > 100));
        }
    }
}
#endif
