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
//
// Authors:
//        Federico Di Gregorio <fog@initd.org>

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions
{    
    // Tests for (type, methodName, ...) version.

    [TestFixture]
    public class ExpressionTest_CallWithType
    {                
        [Test]
        [ExpectedException (typeof (NullReferenceException))]
        public void Arg1Null ()
        {
            Expression.Call ((Type)null, "TestMethod", null, Expression.Constant (1));
        }

        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void Arg2Null ()
        {
            Expression.Call (typeof (MemberClass), null, null, Expression.Constant (1));
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void Arg4WrongType ()
        {
            Expression.Call (typeof (MemberClass), "StaticMethod", null, Expression.Constant (true));
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void InstanceMethod ()
        {
            Expression.Call (typeof (MemberClass), "TestMethod", null, Expression.Constant (1));
        }

        [Test]
        public void StaticMethod ()
        {
            Expression.Call (typeof (MemberClass), "StaticMethod", null, Expression.Constant (1));
        }

        [Test]
        public void StaticGenericMethod ()
        {
            MemberClass.StaticGenericMethod(1);
            Expression.Call (typeof (MemberClass), "StaticGenericMethod", new Type [1] { typeof (int) }, Expression.Constant (1));
        }
    }
}
