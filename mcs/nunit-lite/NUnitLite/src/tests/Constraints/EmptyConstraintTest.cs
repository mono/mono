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
using System.Collections;
using NUnit.TestUtilities;

namespace NUnit.Framework.Constraints.Tests
{
    [TestFixture]
    public class EmptyConstraintTest : ConstraintTestBaseWithArgumentException
    {
        [SetUp]
        public void SetUp()
        {
            theConstraint = new EmptyConstraint();
            expectedDescription = "<empty>";
            stringRepresentation = "<empty>";
        }

        internal static object[] SuccessData = new object[] 
        {
#if CLR_2_0 || CLR_4_0
            new System.Collections.Generic.List<int>(),
#endif
            string.Empty,
            new object[0],
            new SimpleObjectCollection()
        };

        internal static object[] FailureData = new object[]
        {
            new TestCaseData( "Hello", "\"Hello\"" ),
            new TestCaseData( new object[] { 1, 2, 3 }, "< 1, 2, 3 >" )
        };

        internal static object[] InvalidData = new object[]
            {
                null,
                5
            };
    }

    [TestFixture]
    public class NullOrEmptyStringConstraintTest : ConstraintTestBaseWithArgumentException
    {
        [SetUp]
        public void SetUp()
        {
            theConstraint = new NullOrEmptyStringConstraint();
            expectedDescription = "null or empty string";
            stringRepresentation = "<nullorempty>";
        }

        internal static object[] SuccessData = new object[] 
        {
            string.Empty,
            null
        };

        internal static object[] FailureData = new object[]
        {
            new TestCaseData( "Hello", "\"Hello\"" )
        };

        internal static object[] InvalidData = new object[]
            {
                5
            };
    }
}