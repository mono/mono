﻿// ***********************************************************************
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
using NUnit.Framework.Internal;

namespace NUnit.Framework.Constraints.Tests
{
    [TestFixture]
    public class EndsWithConstraintTests : ConstraintTestBase
    {
        [SetUp]
        public void SetUp()
        {
            theConstraint = new EndsWithConstraint("hello");
            expectedDescription = "String ending with \"hello\"";
            stringRepresentation = "<endswith \"hello\">";
        }

        internal object[] SuccessData = new object[] { "hello", "I said hello" };

        internal object[] FailureData = new object[] {
            new TestCaseData( "goodbye", "\"goodbye\"" ), 
            new TestCaseData( "hello there", "\"hello there\"" ),
            new TestCaseData( "say hello to Fred", "\"say hello to Fred\"" ),
            new TestCaseData( string.Empty, "<string.Empty>" ),
            new TestCaseData( null , "null" ) };
    }

    [TestFixture]
    public class EndsWithConstraintTestsIgnoringCase : ConstraintTestBase
    {
        [SetUp]
        public void SetUp()
        {
            theConstraint = new EndsWithConstraint("hello").IgnoreCase;
            expectedDescription = "String ending with \"hello\", ignoring case";
            stringRepresentation = "<endswith \"hello\">";
        }

        internal object[] SuccessData = new object[] { "HELLO", "I said Hello" };

        internal object[] FailureData = new object[] {
            new TestCaseData( "goodbye", "\"goodbye\"" ), 
            new TestCaseData( "What the hell?", "\"What the hell?\"" ),
            new TestCaseData( "hello there", "\"hello there\"" ),
            new TestCaseData( "say hello to Fred", "\"say hello to Fred\"" ),
            new TestCaseData( string.Empty, "<string.Empty>" ),
            new TestCaseData( null , "null" ) };
    }
}
