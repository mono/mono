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
    [TestFixture]
    public class ThrowsTests
    {
        [Test]
        public void ThrowsException()
        {
            IResolveConstraint expr = Throws.Exception;
            Assert.AreEqual(
                "<throws>",
                expr.Resolve().ToString());
        }

        [Test]
        public void ThrowsExceptionWithConstraint()
        {
            IResolveConstraint expr = Throws.Exception.With.Property("ParamName").EqualTo("myParam");
            Assert.AreEqual(
                @"<throws <property ParamName <equal ""myParam"">>>",
                expr.Resolve().ToString());
        }

        [Test]
        public void ThrowsExceptionTypeOf()
        {
            IResolveConstraint expr = Throws.Exception.TypeOf(typeof(ArgumentException));
            Assert.AreEqual(
                "<throws <typeof System.ArgumentException>>",
                expr.Resolve().ToString());
        }

        [Test]
        public void ThrowsTypeOf()
        {
            IResolveConstraint expr = Throws.TypeOf(typeof(ArgumentException));
            Assert.AreEqual(
                "<throws <typeof System.ArgumentException>>",
                expr.Resolve().ToString());
        }

        [Test]
        public void ThrowsTypeOfAndConstraint()
        {
            IResolveConstraint expr = Throws.TypeOf(typeof(ArgumentException)).And.Property("ParamName").EqualTo("myParam");
            Assert.AreEqual(
                @"<throws <and <typeof System.ArgumentException> <property ParamName <equal ""myParam"">>>>",
                expr.Resolve().ToString());
        }

        [Test]
        public void ThrowsExceptionTypeOfAndConstraint()
        {
            IResolveConstraint expr = Throws.Exception.TypeOf(typeof(ArgumentException)).And.Property("ParamName").EqualTo("myParam");
            Assert.AreEqual(
                @"<throws <and <typeof System.ArgumentException> <property ParamName <equal ""myParam"">>>>",
                expr.Resolve().ToString());
        }

        [Test]
        public void ThrowsTypeOfWithConstraint()
        {
            IResolveConstraint expr = Throws.TypeOf(typeof(ArgumentException)).With.Property("ParamName").EqualTo("myParam");
            Assert.AreEqual(
                @"<throws <and <typeof System.ArgumentException> <property ParamName <equal ""myParam"">>>>",
                expr.Resolve().ToString());
        }

        [Test]
        public void ThrowsTypeofWithMessage()
        {
            IResolveConstraint expr = Throws.TypeOf(typeof(ArgumentException)).With.Message.EqualTo("my message");
            Assert.AreEqual(
                @"<throws <and <typeof System.ArgumentException> <property Message <equal ""my message"">>>>",
                expr.Resolve().ToString());
        }

        [Test]
        public void ThrowsInstanceOf()
        {
            IResolveConstraint expr = Throws.InstanceOf(typeof(ArgumentException));
            Assert.AreEqual(
                "<throws <instanceof System.ArgumentException>>",
                expr.Resolve().ToString());
        }

        [Test]
        public void ThrowsExceptionInstanceOf()
        {
            IResolveConstraint expr = Throws.Exception.InstanceOf(typeof(ArgumentException));
            Assert.AreEqual(
                "<throws <instanceof System.ArgumentException>>",
                expr.Resolve().ToString());
        }

        [Test]
        public void ThrowsInnerException()
        {
            IResolveConstraint expr = Throws.InnerException.TypeOf(typeof(ArgumentException));
            Assert.AreEqual(
                "<throws <property InnerException <typeof System.ArgumentException>>>",
                expr.Resolve().ToString());
        }

        [Test]
        public void ThrowsExceptionWithInnerException()
        {
            IResolveConstraint expr = Throws.Exception.With.InnerException.TypeOf(typeof(ArgumentException));
            Assert.AreEqual(
                "<throws <property InnerException <typeof System.ArgumentException>>>",
                expr.Resolve().ToString());
        }

        [Test]
        public void ThrowsTypeOfWithInnerException()
        {
            IResolveConstraint expr = Throws.TypeOf(typeof(System.Reflection.TargetInvocationException))
                .With.InnerException.TypeOf(typeof(ArgumentException));
            Assert.AreEqual(
                "<throws <and <typeof System.Reflection.TargetInvocationException> <property InnerException <typeof System.ArgumentException>>>>",
                expr.Resolve().ToString());
        }

        [Test]
        public void ThrowsTargetInvocationExceptionWithInnerException()
        {
            IResolveConstraint expr = Throws.TargetInvocationException
                .With.InnerException.TypeOf(typeof(ArgumentException));
            Assert.AreEqual(
                "<throws <and <typeof System.Reflection.TargetInvocationException> <property InnerException <typeof System.ArgumentException>>>>",
                expr.Resolve().ToString());
        }

        [Test]
        public void ThrowsArgumentException()
        {
            IResolveConstraint expr = Throws.ArgumentException;
            Assert.AreEqual(
                "<throws <typeof System.ArgumentException>>",
                expr.Resolve().ToString());
        }

        [Test]
        public void ThrowsInvalidOperationException()
        {
            IResolveConstraint expr = Throws.InvalidOperationException;
            Assert.AreEqual(
                "<throws <typeof System.InvalidOperationException>>",
                expr.Resolve().ToString());
        }

#if CLR_2_0 || CLR_4_0
#if !NETCF
        [Test]
        public void DelegateThrowsException()
        {
            Assert.That(
                delegate { throw new ArgumentException(); },
                Throws.Exception);
        }

        [Test]
        public void LambdaThrowsExcepton()
        {
            Assert.That(
                () => new MyClass(null),
                Throws.InstanceOf<ArgumentNullException>());
        }

        [Test]
        public void LambdaThrowsExceptionWithMessage()
        {
            Assert.That(
                () => new MyClass(null),
                Throws.InstanceOf<ArgumentNullException>()
                .And.Message.Matches("null"));
        }

        internal class MyClass
        {
            public MyClass(string s)
            {
                if (s == null)
                {
                    throw new ArgumentNullException();
                }
            }
        }

        [Test]
        public void LambdaThrowsNothing()
        {
            Assert.That(() => (object)null, Throws.Nothing);
        }
#else
        [Test]
        public void DelegateThrowsException()
        {
            Assert.That(
                delegate { Throw(); return; },
                Throws.Exception);
        }

        // Encapsulate throw to trick compiler and
        // avoid unreachable code warning. Can't
        // use pragma because this is also compiled
        // under the .NET 1.0 and 1.1 compilers.
        private void Throw()
        {
            throw new ApplicationException();
        }
#endif
#endif
    }
}
