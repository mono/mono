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

namespace NUnit.Framework.Constraints.Tests
{
    [TestFixture]
    public class ExactTypeConstraintTests : ConstraintTestBase
    {
        [SetUp]
        public void SetUp()
        {
            theConstraint = new ExactTypeConstraint(typeof(D1));
            expectedDescription = string.Format("<{0}>", typeof(D1));
            stringRepresentation = string.Format("<typeof {0}>", typeof(D1));
        }

        internal object[] SuccessData = new object[] { new D1() };

        internal object[] FailureData = new object[] { 
            new TestCaseData( new B(), "<NUnit.Framework.Constraints.Tests.ExactTypeConstraintTests+B>" ),
            new TestCaseData( new D2(), "<NUnit.Framework.Constraints.Tests.ExactTypeConstraintTests+D2>" )
        };

        class B { }

        class D1 : B { }

        class D2 : D1 { }
    }
}