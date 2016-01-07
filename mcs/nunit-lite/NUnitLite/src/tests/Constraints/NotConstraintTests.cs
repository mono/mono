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

using NUnit.Framework.Constraints;

namespace NUnit.Framework.Constraints.Tests
{
    [TestFixture]
    public class NotConstraintTests : ConstraintTestBase
    {
        [SetUp]
        public void SetUp()
        {
            theConstraint = new NotConstraint( new EqualConstraint(null) );
            expectedDescription = "not null";
            stringRepresentation = "<not <equal null>>";
        }

        internal object[] SuccessData = new object[] { 42, "Hello" };

        internal object[] FailureData = new object[] { new object[] { null, "null" } };

        [Test, ExpectedException(typeof(AssertionException), ExpectedMessage = "ignoring case", MatchType = MessageMatch.Contains)]
        public void NotHonorsIgnoreCaseUsingConstructors()
        {
            Assert.That("abc", new NotConstraint(new EqualConstraint("ABC").IgnoreCase));
        }

        [Test,ExpectedException(typeof(AssertionException),ExpectedMessage="ignoring case",MatchType=MessageMatch.Contains)]
        public void NotHonorsIgnoreCaseUsingPrefixNotation()
        {
            Assert.That( "abc", Is.Not.EqualTo( "ABC" ).IgnoreCase );
        }

        [Test,ExpectedException(typeof(AssertionException),ExpectedMessage="+/-",MatchType=MessageMatch.Contains)]
        public void NotHonorsTolerance()
        {
            Assert.That( 4.99d, Is.Not.EqualTo( 5.0d ).Within( .05d ) );
        }

        [Test]
        public void CanUseNotOperator()
        {
            Assert.That(42, !new EqualConstraint(99));
        }
    }
}