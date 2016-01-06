// ***********************************************************************
// Copyright (c) 2011 Charlie Poole
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
using System.ComponentModel;
using NUnit.Framework.Constraints;
using NUnit.Framework.Internal;

namespace NUnit.Framework
{
    /// <summary>
    /// Delegate used by tests that execute code and
    /// capture any thrown exception.
    /// </summary>
    public delegate void TestDelegate();

    /// <summary>
    /// The Assert class contains a collection of static methods that
    /// implement the most common assertions used in NUnit.
    /// </summary>
    public class Assert
    {
        #region Constructor

        /// <summary>
        /// We don't actually want any instances of this object, but some people
        /// like to inherit from it to add other static methods. Hence, the
        /// protected constructor disallows any instances of this object. 
        /// </summary>
        protected Assert() { }

        #endregion

        #region Equals and ReferenceEquals

#if !NETCF
        /// <summary>
        /// The Equals method throws an AssertionException. This is done 
        /// to make sure there is no mistake by calling this function.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static new bool Equals(object a, object b)
        {
            throw new InvalidOperationException("Assert.Equals should not be used for Assertions");
        }

        /// <summary>
        /// override the default ReferenceEquals to throw an AssertionException. This 
        /// implementation makes sure there is no mistake in calling this function 
        /// as part of Assert. 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static new void ReferenceEquals(object a, object b)
        {
            throw new InvalidOperationException("Assert.ReferenceEquals should not be used for Assertions");
        }
#endif

        #endregion

        #region Utility Asserts

        #region Pass

        /// <summary>
        /// Throws a <see cref="SuccessException"/> with the message and arguments 
        /// that are passed in. This allows a test to be cut short, with a result
        /// of success returned to NUnit.
        /// </summary>
        /// <param name="message">The message to initialize the <see cref="AssertionException"/> with.</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        static public void Pass(string message, params object[] args)
        {
            if (message == null) message = string.Empty;
            else if (args != null && args.Length > 0)
                message = string.Format(message, args);

            throw new SuccessException(message);
        }

        /// <summary>
        /// Throws a <see cref="SuccessException"/> with the message and arguments 
        /// that are passed in. This allows a test to be cut short, with a result
        /// of success returned to NUnit.
        /// </summary>
        /// <param name="message">The message to initialize the <see cref="AssertionException"/> with.</param>
        static public void Pass(string message)
        {
            Assert.Pass(message, null);
        }

        /// <summary>
        /// Throws a <see cref="SuccessException"/> with the message and arguments 
        /// that are passed in. This allows a test to be cut short, with a result
        /// of success returned to NUnit.
        /// </summary>
        static public void Pass()
        {
            Assert.Pass(string.Empty, null);
        }

        #endregion

        #region Fail

        /// <summary>
        /// Throws an <see cref="AssertionException"/> with the message and arguments 
        /// that are passed in. This is used by the other Assert functions. 
        /// </summary>
        /// <param name="message">The message to initialize the <see cref="AssertionException"/> with.</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        static public void Fail(string message, params object[] args)
        {
            if (message == null) message = string.Empty;
            else if (args != null && args.Length > 0)
                message = string.Format(message, args);

            throw new AssertionException(message);
        }

        /// <summary>
        /// Throws an <see cref="AssertionException"/> with the message that is 
        /// passed in. This is used by the other Assert functions. 
        /// </summary>
        /// <param name="message">The message to initialize the <see cref="AssertionException"/> with.</param>
        static public void Fail(string message)
        {
            Assert.Fail(message, null);
        }

        /// <summary>
        /// Throws an <see cref="AssertionException"/>. 
        /// This is used by the other Assert functions. 
        /// </summary>
        static public void Fail()
        {
            Assert.Fail(string.Empty, null);
        }

        #endregion

        #region Ignore

        /// <summary>
        /// Throws an <see cref="IgnoreException"/> with the message and arguments 
        /// that are passed in.  This causes the test to be reported as ignored.
        /// </summary>
        /// <param name="message">The message to initialize the <see cref="AssertionException"/> with.</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        static public void Ignore(string message, params object[] args)
        {
            if (message == null) message = string.Empty;
            else if (args != null && args.Length > 0)
                message = string.Format(message, args);

            throw new IgnoreException(message);
        }

        /// <summary>
        /// Throws an <see cref="IgnoreException"/> with the message that is 
        /// passed in. This causes the test to be reported as ignored. 
        /// </summary>
        /// <param name="message">The message to initialize the <see cref="AssertionException"/> with.</param>
        static public void Ignore(string message)
        {
            Assert.Ignore(message, null);
        }

        /// <summary>
        /// Throws an <see cref="IgnoreException"/>. 
        /// This causes the test to be reported as ignored. 
        /// </summary>
        static public void Ignore()
        {
            Assert.Ignore(string.Empty, null);
        }

        #endregion

        #region InConclusive

        /// <summary>
        /// Throws an <see cref="InconclusiveException"/> with the message and arguments 
        /// that are passed in.  This causes the test to be reported as inconclusive.
        /// </summary>
        /// <param name="message">The message to initialize the <see cref="InconclusiveException"/> with.</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        static public void Inconclusive(string message, params object[] args)
        {
            if (message == null) message = string.Empty;
            else if (args != null && args.Length > 0)
                message = string.Format(message, args);

            throw new InconclusiveException(message);
        }

        /// <summary>
        /// Throws an <see cref="InconclusiveException"/> with the message that is 
        /// passed in. This causes the test to be reported as inconclusive. 
        /// </summary>
        /// <param name="message">The message to initialize the <see cref="InconclusiveException"/> with.</param>
        static public void Inconclusive(string message)
        {
            Assert.Inconclusive(message, null);
        }

        /// <summary>
        /// Throws an <see cref="InconclusiveException"/>. 
        /// This causes the test to be reported as Inconclusive. 
        /// </summary>
        static public void Inconclusive()
        {
            Assert.Inconclusive(string.Empty, null);
        }

        #endregion

        #endregion

        #region Assert.That

        #region Object

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="actual">The actual value to test</param>
        /// <param name="expression">A Constraint to be applied</param>
        static public void That(object actual, IResolveConstraint expression)
        {
            Assert.That(actual, expression, null, null);
        }

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="actual">The actual value to test</param>
        /// <param name="expression">A Constraint to be applied</param>
        /// <param name="message">The message that will be displayed on failure</param>
        static public void That(object actual, IResolveConstraint expression, string message)
        {
            Assert.That(actual, expression, message, null);
        }

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="actual">The actual value to test</param>
        /// <param name="expression">A Constraint expression to be applied</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        static public void That(object actual, IResolveConstraint expression, string message, params object[] args)
        {
            Constraint constraint = expression.Resolve();

            IncrementAssertCount();
            if (!constraint.Matches(actual))
            {
                MessageWriter writer = new TextMessageWriter(message, args);
                constraint.WriteMessageTo(writer);
                throw new AssertionException(writer.ToString());
            }
        }

        #endregion

        #region Boolean

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary> 
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display if the condition is false</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        static public void That(bool condition, string message, params object[] args)
        {
            Assert.That(condition, Is.True, message, args);
        }

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display if the condition is false</param>
        static public void That(bool condition, string message)
        {
            Assert.That(condition, Is.True, message, null);
        }

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="condition">The evaluated condition</param>
        static public void That(bool condition)
        {
            Assert.That(condition, Is.True, null, null);
        }

        #endregion

        #region ref Boolean

#if !CLR_2_0 && !CLR_4_0
            /// <summary>
            /// Apply a constraint to a referenced boolean, succeeding if the constraint
            /// is satisfied and throwing an assertion exception on failure.
            /// </summary>
            /// <param name="actual">The actual value to test</param>
            /// <param name="constraint">A Constraint to be applied</param>
            static public void That(ref bool actual, IResolveConstraint constraint)
            {
                Assert.That(ref actual, constraint.Resolve(), null, null);
            }

            /// <summary>
            /// Apply a constraint to a referenced value, succeeding if the constraint
            /// is satisfied and throwing an assertion exception on failure.
            /// </summary>
            /// <param name="actual">The actual value to test</param>
            /// <param name="constraint">A Constraint to be applied</param>
            /// <param name="message">The message that will be displayed on failure</param>
            static public void That(ref bool actual, IResolveConstraint constraint, string message)
            {
                Assert.That(ref actual, constraint.Resolve(), message, null);
            }

            /// <summary>
            /// Apply a constraint to a referenced value, succeeding if the constraint
            /// is satisfied and throwing an assertion exception on failure.
            /// </summary>
            /// <param name="actual">The actual value to test</param>
            /// <param name="expression">A Constraint expression to be applied</param>
            /// <param name="message">The message that will be displayed on failure</param>
            /// <param name="args">Arguments to be used in formatting the message</param>
            static public void That(ref bool actual, IResolveConstraint expression, string message, params object[] args)
            {
                Constraint constraint = expression.Resolve();

                IncrementAssertCount();
                if (!constraint.Matches(ref actual))
                {
                    MessageWriter writer = new TextMessageWriter(message, args);
                    constraint.WriteMessageTo(writer);
                    throw new AssertionException(writer.ToString());
                }
            }
#endif

        #endregion

        #region ActualValueDelegate

#if CLR_2_0 || CLR_4_0
        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="del">An ActualValueDelegate returning the value to be tested</param>
        /// <param name="expr">A Constraint expression to be applied</param>
        static public void That<T>(ActualValueDelegate<T> del, IResolveConstraint expr)
        {
            Assert.That(del, expr.Resolve(), null, null);
        }

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="del">An ActualValueDelegate returning the value to be tested</param>
        /// <param name="expr">A Constraint expression to be applied</param>
        /// <param name="message">The message that will be displayed on failure</param>
        static public void That<T>(ActualValueDelegate<T> del, IResolveConstraint expr, string message)
        {
            Assert.That(del, expr.Resolve(), message, null);
        }

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="del">An ActualValueDelegate returning the value to be tested</param>
        /// <param name="expr">A Constraint expression to be applied</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        static public void That<T>(ActualValueDelegate<T> del, IResolveConstraint expr, string message, params object[] args)
        {
            Constraint constraint = expr.Resolve();

            IncrementAssertCount();
            if (!constraint.Matches(del))
            {
                MessageWriter writer = new TextMessageWriter(message, args);
                constraint.WriteMessageTo(writer);
                throw new AssertionException(writer.ToString());
            }
        }
#else
        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="del">An ActualValueDelegate returning the value to be tested</param>
        /// <param name="expr">A Constraint expression to be applied</param>
        static public void That(ActualValueDelegate del, IResolveConstraint expr)
        {
            Assert.That(del, expr.Resolve(), null, null);
        }

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="del">An ActualValueDelegate returning the value to be tested</param>
        /// <param name="expr">A Constraint expression to be applied</param>
        /// <param name="message">The message that will be displayed on failure</param>
        static public void That(ActualValueDelegate del, IResolveConstraint expr, string message)
        {
            Assert.That(del, expr.Resolve(), message, null);
        }

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="del">An ActualValueDelegate returning the value to be tested</param>
        /// <param name="expr">A Constraint expression to be applied</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        static public void That(ActualValueDelegate del, IResolveConstraint expr, string message, params object[] args)
        {
            Constraint constraint = expr.Resolve();

            IncrementAssertCount();
            if (!constraint.Matches(del))
            {
                MessageWriter writer = new TextMessageWriter(message, args);
                constraint.WriteMessageTo(writer);
                throw new AssertionException(writer.ToString());
            }
        }
#endif

        #endregion

        #region ref Object

#if CLR_2_0 || CLR_4_0
        /// <summary>
        /// Apply a constraint to a referenced value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="actual">The actual value to test</param>
        /// <param name="expression">A Constraint to be applied</param>
        static public void That<T>(ref T actual, IResolveConstraint expression)
        {
            Assert.That(ref actual, expression, null, null);
        }

        /// <summary>
        /// Apply a constraint to a referenced value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="actual">The actual value to test</param>
        /// <param name="expression">A Constraint to be applied</param>
        /// <param name="message">The message that will be displayed on failure</param>
        static public void That<T>(ref T actual, IResolveConstraint expression, string message)
        {
            Assert.That(ref actual, expression, message, null);
        }

        /// <summary>
        /// Apply a constraint to a referenced value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <param name="actual">The actual value to test</param>
        /// <param name="expression">A Constraint to be applied</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        static public void That<T>(ref T actual, IResolveConstraint expression, string message, params object[] args)
        {
            Constraint constraint = expression.Resolve();

            IncrementAssertCount();
            if (!constraint.Matches(ref actual))
            {
                MessageWriter writer = new TextMessageWriter(message, args);
                constraint.WriteMessageTo(writer);
                throw new AssertionException(writer.ToString());
            }
        }
#endif

        #endregion

        #region TestDelegate

        /// <summary>
        /// Asserts that the code represented by a delegate throws an exception
        /// that satisfies the constraint provided.
        /// </summary>
        /// <param name="code">A TestDelegate to be executed</param>
        /// <param name="constraint">A ThrowsConstraint used in the test</param>
        static public void That(TestDelegate code, IResolveConstraint constraint)
        {
            Assert.That((object)code, constraint);
        }

        #endregion

        #endregion

        #region Assert.ByVal

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// Used as a synonym for That in rare cases where a private setter 
        /// causes a Visual Basic compilation error.
        /// </summary>
        /// <param name="actual">The actual value to test</param>
        /// <param name="expression">A Constraint to be applied</param>
        static public void ByVal(object actual, IResolveConstraint expression)
        {
            Assert.That(actual, expression, null, null);
        }

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// Used as a synonym for That in rare cases where a private setter 
        /// causes a Visual Basic compilation error.
        /// </summary>
        /// <param name="actual">The actual value to test</param>
        /// <param name="expression">A Constraint to be applied</param>
        /// <param name="message">The message that will be displayed on failure</param>
        static public void ByVal(object actual, IResolveConstraint expression, string message)
        {
            Assert.That(actual, expression, message, null);
        }

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure. 
        /// Used as a synonym for That in rare cases where a private setter 
        /// causes a Visual Basic compilation error.
        /// </summary>
        /// <remarks>
        /// This method is provided for use by VB developers needing to test
        /// the value of properties with private setters.
        /// </remarks>
        /// <param name="actual">The actual value to test</param>
        /// <param name="expression">A Constraint expression to be applied</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        static public void ByVal(object actual, IResolveConstraint expression, string message, params object[] args)
        {
            Assert.That(actual, expression, message, args);
        }

        #endregion

        #region Throws, Catch and DoesNotThrow

        #region Throws
        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <param name="expression">A constraint to be satisfied by the exception</param>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static Exception Throws(IResolveConstraint expression, TestDelegate code, string message, params object[] args)
        {
            Exception caughtException = null;

#if NET_4_5
            if (AsyncInvocationRegion.IsAsyncOperation(code))
            {
                using (AsyncInvocationRegion region = AsyncInvocationRegion.Create(code))
                {
                    code();

                    try
                    {
                        region.WaitForPendingOperationsToComplete(null);
                    }
                    catch (Exception e)
                    {
                        caughtException = e;
                    }
                }
            }
            else
#endif
                try
            {
                code();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.That(caughtException, expression, message, args);

            return caughtException;
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <param name="expression">A constraint to be satisfied by the exception</param>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        public static Exception Throws(IResolveConstraint expression, TestDelegate code, string message)
        {
            return Throws(expression, code, message, null);
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <param name="expression">A constraint to be satisfied by the exception</param>
        /// <param name="code">A TestDelegate</param>
        public static Exception Throws(IResolveConstraint expression, TestDelegate code)
        {
            return Throws(expression, code, string.Empty, null);
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <param name="expectedExceptionType">The exception Type expected</param>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static Exception Throws(Type expectedExceptionType, TestDelegate code, string message, params object[] args)
        {
            return Throws(new ExceptionTypeConstraint(expectedExceptionType), code, message, args);
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <param name="expectedExceptionType">The exception Type expected</param>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        public static Exception Throws(Type expectedExceptionType, TestDelegate code, string message)
        {
            return Throws(new ExceptionTypeConstraint(expectedExceptionType), code, message, null);
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <param name="expectedExceptionType">The exception Type expected</param>
        /// <param name="code">A TestDelegate</param>
        public static Exception Throws(Type expectedExceptionType, TestDelegate code)
        {
            return Throws(new ExceptionTypeConstraint(expectedExceptionType), code, string.Empty, null);
        }

        #endregion

        #region Throws<T>

#if CLR_2_0 || CLR_4_0
        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <typeparam name="T">Type of the expected exception</typeparam>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static T Throws<T>(TestDelegate code, string message, params object[] args) where T : Exception
        {
            return (T)Throws(typeof(T), code, message, args);
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <typeparam name="T">Type of the expected exception</typeparam>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        public static T Throws<T>(TestDelegate code, string message) where T : Exception
        {
            return Throws<T>(code, message, null);
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <typeparam name="T">Type of the expected exception</typeparam>
        /// <param name="code">A TestDelegate</param>
        public static T Throws<T>(TestDelegate code) where T : Exception
        {
            return Throws<T>(code, string.Empty, null);
        }
#endif

        #endregion

        #region Catch
        /// <summary>
        /// Verifies that a delegate throws an exception when called
        /// and returns it.
        /// </summary>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static Exception Catch(TestDelegate code, string message, params object[] args)
        {
            return Throws(new InstanceOfTypeConstraint(typeof(Exception)), code, message, args);
        }

        /// <summary>
        /// Verifies that a delegate throws an exception when called
        /// and returns it.
        /// </summary>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        public static Exception Catch(TestDelegate code, string message)
        {
            return Throws(new InstanceOfTypeConstraint(typeof(Exception)), code, message);
        }

        /// <summary>
        /// Verifies that a delegate throws an exception when called
        /// and returns it.
        /// </summary>
        /// <param name="code">A TestDelegate</param>
        public static Exception Catch(TestDelegate code)
        {
            return Throws(new InstanceOfTypeConstraint(typeof(Exception)), code);
        }

        /// <summary>
        /// Verifies that a delegate throws an exception of a certain Type
        /// or one derived from it when called and returns it.
        /// </summary>
        /// <param name="expectedExceptionType">The expected Exception Type</param>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static Exception Catch(Type expectedExceptionType, TestDelegate code, string message, params object[] args)
        {
            return Throws(new InstanceOfTypeConstraint(expectedExceptionType), code, message, args);
        }

        /// <summary>
        /// Verifies that a delegate throws an exception of a certain Type
        /// or one derived from it when called and returns it.
        /// </summary>
        /// <param name="expectedExceptionType">The expected Exception Type</param>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        public static Exception Catch(Type expectedExceptionType, TestDelegate code, string message)
        {
            return Throws(new InstanceOfTypeConstraint(expectedExceptionType), code, message);
        }

        /// <summary>
        /// Verifies that a delegate throws an exception of a certain Type
        /// or one derived from it when called and returns it.
        /// </summary>
        /// <param name="expectedExceptionType">The expected Exception Type</param>
        /// <param name="code">A TestDelegate</param>
        public static Exception Catch(Type expectedExceptionType, TestDelegate code)
        {
            return Throws(new InstanceOfTypeConstraint(expectedExceptionType), code);
        }
        #endregion

        #region Catch<T>

#if CLR_2_0 || CLR_4_0
        /// <summary>
        /// Verifies that a delegate throws an exception of a certain Type
        /// or one derived from it when called and returns it.
        /// </summary>
        /// <typeparam name="T">The expected Exception Type</typeparam>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static T Catch<T>(TestDelegate code, string message, params object[] args) where T : System.Exception
        {
            return (T)Throws(new InstanceOfTypeConstraint(typeof(T)), code, message, args);
        }

        /// <summary>
        /// Verifies that a delegate throws an exception of a certain Type
        /// or one derived from it when called and returns it.
        /// </summary>
        /// <typeparam name="T">The expected Exception Type</typeparam>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        public static T Catch<T>(TestDelegate code, string message) where T : System.Exception
        {
            return (T)Throws(new InstanceOfTypeConstraint(typeof(T)), code, message);
        }

        /// <summary>
        /// Verifies that a delegate throws an exception of a certain Type
        /// or one derived from it when called and returns it.
        /// </summary>
        /// <typeparam name="T">The expected Exception Type</typeparam>
        /// <param name="code">A TestDelegate</param>
        public static T Catch<T>(TestDelegate code) where T : System.Exception
        {
            return (T)Throws(new InstanceOfTypeConstraint(typeof(T)), code);
        }
#endif

        #endregion

        #region DoesNotThrow

        /// <summary>
        /// Verifies that a delegate does not throw an exception
        /// </summary>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static void DoesNotThrow(TestDelegate code, string message, params object[] args)
        {
            Assert.That(code, new ThrowsNothingConstraint(), message, args);
        }

        /// <summary>
        /// Verifies that a delegate does not throw an exception.
        /// </summary>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        public static void DoesNotThrow(TestDelegate code, string message)
        {
            DoesNotThrow(code, message, null);
        }

        /// <summary>
        /// Verifies that a delegate does not throw an exception.
        /// </summary>
        /// <param name="code">A TestDelegate</param>
        public static void DoesNotThrow(TestDelegate code)
        {
            DoesNotThrow(code, string.Empty, null);
        }

        #endregion

        #endregion

        #region True

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void True(bool condition, string message, params object[] args)
        {
            Assert.That(condition, Is.True ,message, args);
        }
        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void True(bool condition, string message)
        {
            Assert.That(condition, Is.True ,message, null);
        }
        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="condition">The evaluated condition</param>
        public static void True(bool condition)
        {
            Assert.That(condition, Is.True ,null, null);
        }

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsTrue(bool condition, string message, params object[] args)
        {
            Assert.That(condition, Is.True ,message, args);
        }
        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void IsTrue(bool condition, string message)
        {
            Assert.That(condition, Is.True ,message, null);
        }
        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="condition">The evaluated condition</param>
        public static void IsTrue(bool condition)
        {
            Assert.That(condition, Is.True ,null, null);
        }

        #endregion

        #region False

        /// <summary>
        /// Asserts that a condition is false. If the condition is true the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary> 
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void False(bool condition, string message, params object[] args)
        {
            Assert.That(condition, Is.False ,message, args);
        }
        /// <summary>
        /// Asserts that a condition is false. If the condition is true the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary> 
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void False(bool condition, string message)
        {
            Assert.That(condition, Is.False ,message, null);
        }
        /// <summary>
        /// Asserts that a condition is false. If the condition is true the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary> 
        /// <param name="condition">The evaluated condition</param>
        public static void False(bool condition)
        {
            Assert.That(condition, Is.False ,null, null);
        }

        /// <summary>
        /// Asserts that a condition is false. If the condition is true the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary> 
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsFalse(bool condition, string message, params object[] args)
        {
            Assert.That(condition, Is.False ,message, args);
        }
        /// <summary>
        /// Asserts that a condition is false. If the condition is true the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary> 
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void IsFalse(bool condition, string message)
        {
            Assert.That(condition, Is.False ,message, null);
        }
        /// <summary>
        /// Asserts that a condition is false. If the condition is true the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary> 
        /// <param name="condition">The evaluated condition</param>
        public static void IsFalse(bool condition)
        {
            Assert.That(condition, Is.False ,null, null);
        }

        #endregion

        #region NotNull

        /// <summary>
        /// Verifies that the object that is passed in is not equal to <code>null</code>
        /// If the object is <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="anObject">The object that is to be tested</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void NotNull(object anObject, string message, params object[] args)
        {
            Assert.That(anObject, Is.Not.Null ,message, args);
        }
        /// <summary>
        /// Verifies that the object that is passed in is not equal to <code>null</code>
        /// If the object is <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="anObject">The object that is to be tested</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void NotNull(object anObject, string message)
        {
            Assert.That(anObject, Is.Not.Null ,message, null);
        }
        /// <summary>
        /// Verifies that the object that is passed in is not equal to <code>null</code>
        /// If the object is <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="anObject">The object that is to be tested</param>
        public static void NotNull(object anObject)
        {
            Assert.That(anObject, Is.Not.Null ,null, null);
        }

        /// <summary>
        /// Verifies that the object that is passed in is not equal to <code>null</code>
        /// If the object is <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="anObject">The object that is to be tested</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsNotNull(object anObject, string message, params object[] args)
        {
            Assert.That(anObject, Is.Not.Null ,message, args);
        }
        /// <summary>
        /// Verifies that the object that is passed in is not equal to <code>null</code>
        /// If the object is <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="anObject">The object that is to be tested</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void IsNotNull(object anObject, string message)
        {
            Assert.That(anObject, Is.Not.Null ,message, null);
        }
        /// <summary>
        /// Verifies that the object that is passed in is not equal to <code>null</code>
        /// If the object is <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="anObject">The object that is to be tested</param>
        public static void IsNotNull(object anObject)
        {
            Assert.That(anObject, Is.Not.Null ,null, null);
        }

        #endregion

        #region Null

        /// <summary>
        /// Verifies that the object that is passed in is equal to <code>null</code>
        /// If the object is not <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="anObject">The object that is to be tested</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Null(object anObject, string message, params object[] args)
        {
            Assert.That(anObject, Is.Null ,message, args);
        }
        /// <summary>
        /// Verifies that the object that is passed in is equal to <code>null</code>
        /// If the object is not <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="anObject">The object that is to be tested</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void Null(object anObject, string message)
        {
            Assert.That(anObject, Is.Null ,message, null);
        }
        /// <summary>
        /// Verifies that the object that is passed in is equal to <code>null</code>
        /// If the object is not <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="anObject">The object that is to be tested</param>
        public static void Null(object anObject)
        {
            Assert.That(anObject, Is.Null ,null, null);
        }

        /// <summary>
        /// Verifies that the object that is passed in is equal to <code>null</code>
        /// If the object is not <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="anObject">The object that is to be tested</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsNull(object anObject, string message, params object[] args)
        {
            Assert.That(anObject, Is.Null ,message, args);
        }
        /// <summary>
        /// Verifies that the object that is passed in is equal to <code>null</code>
        /// If the object is not <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="anObject">The object that is to be tested</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void IsNull(object anObject, string message)
        {
            Assert.That(anObject, Is.Null ,message, null);
        }
        /// <summary>
        /// Verifies that the object that is passed in is equal to <code>null</code>
        /// If the object is not <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="anObject">The object that is to be tested</param>
        public static void IsNull(object anObject)
        {
            Assert.That(anObject, Is.Null ,null, null);
        }

        #endregion

        #region AreEqual

        #region Ints

        /// <summary>
        /// Verifies that two ints are equal. If they are not, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void AreEqual(int expected, int actual, string message, params object[] args)
        {
            Assert.That(actual, Is.EqualTo(expected), message, args);
        }
        /// <summary>
        /// Verifies that two ints are equal. If they are not, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void AreEqual(int expected, int actual, string message)
        {
            Assert.That(actual, Is.EqualTo(expected), message, null);
        }
        /// <summary>
        /// Verifies that two ints are equal. If they are not, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        public static void AreEqual(int expected, int actual)
        {
            Assert.That(actual, Is.EqualTo(expected), null, null);
        }

        #endregion

        #region Longs

        /// <summary>
        /// Verifies that two longs are equal. If they are not, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void AreEqual(long expected, long actual, string message, params object[] args)
        {
            Assert.That(actual, Is.EqualTo(expected), message, args);
        }
        /// <summary>
        /// Verifies that two longs are equal. If they are not, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void AreEqual(long expected, long actual, string message)
        {
            Assert.That(actual, Is.EqualTo(expected), message, null);
        }
        /// <summary>
        /// Verifies that two longs are equal. If they are not, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        public static void AreEqual(long expected, long actual)
        {
            Assert.That(actual, Is.EqualTo(expected), null, null);
        }

        #endregion

        #region Unsigned Ints

        /// <summary>
        /// Verifies that two unsigned ints are equal. If they are not, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        [CLSCompliant(false)]
        public static void AreEqual(uint expected, uint actual, string message, params object[] args)
        {
            Assert.That(actual, Is.EqualTo(expected), message, args);
        }
        /// <summary>
        /// Verifies that two unsigned ints are equal. If they are not, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        [CLSCompliant(false)]
        public static void AreEqual(uint expected, uint actual, string message)
        {
            Assert.That(actual, Is.EqualTo(expected), message, null);
        }
        /// <summary>
        /// Verifies that two unsigned ints are equal. If they are not, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        [CLSCompliant(false)]
        public static void AreEqual(uint expected, uint actual)
        {
            Assert.That(actual, Is.EqualTo(expected), null, null);
        }

        #endregion

        #region Unsigned Longs

        /// <summary>
        /// Verifies that two unsigned longs are equal. If they are not, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        [CLSCompliant(false)]
        public static void AreEqual(ulong expected, ulong actual, string message, params object[] args)
        {
            Assert.That(actual, Is.EqualTo(expected), message, args);
        }
        /// <summary>
        /// Verifies that two unsigned longs are equal. If they are not, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        [CLSCompliant(false)]
        public static void AreEqual(ulong expected, ulong actual, string message)
        {
            Assert.That(actual, Is.EqualTo(expected), message, null);
        }
        /// <summary>
        /// Verifies that two unsigned longs are equal. If they are not, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        [CLSCompliant(false)]
        public static void AreEqual(ulong expected, ulong actual)
        {
            Assert.That(actual, Is.EqualTo(expected), null, null);
        }

        #endregion

        #region Decimals

        /// <summary>
        /// Verifies that two decimals are equal. If they are not, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void AreEqual(decimal expected, decimal actual, string message, params object[] args)
        {
            Assert.That(actual, Is.EqualTo(expected), message, args);
        }
        /// <summary>
        /// Verifies that two decimals are equal. If they are not, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void AreEqual(decimal expected, decimal actual, string message)
        {
            Assert.That(actual, Is.EqualTo(expected), message, null);
        }
        /// <summary>
        /// Verifies that two decimals are equal. If they are not, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        public static void AreEqual(decimal expected, decimal actual)
        {
            Assert.That(actual, Is.EqualTo(expected), null, null);
        }

        #endregion

        #region Doubles

        /// <summary>
        /// Verifies that two doubles are equal considering a delta. If the
        /// expected value is infinity then the delta value is ignored. If 
        /// they are not equal then an <see cref="AssertionException"/> is
        /// thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="delta">The maximum acceptable difference between the
        /// the expected and the actual</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void AreEqual(double expected, double actual, double delta, string message, params object[] args)
        {
            AssertDoublesAreEqual(expected, actual, delta, message, args);
        }
        /// <summary>
        /// Verifies that two doubles are equal considering a delta. If the
        /// expected value is infinity then the delta value is ignored. If 
        /// they are not equal then an <see cref="AssertionException"/> is
        /// thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="delta">The maximum acceptable difference between the
        /// the expected and the actual</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void AreEqual(double expected, double actual, double delta, string message)
        {
            AssertDoublesAreEqual(expected, actual, delta, message, null);
        }
        /// <summary>
        /// Verifies that two doubles are equal considering a delta. If the
        /// expected value is infinity then the delta value is ignored. If 
        /// they are not equal then an <see cref="AssertionException"/> is
        /// thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="delta">The maximum acceptable difference between the
        /// the expected and the actual</param>
        public static void AreEqual(double expected, double actual, double delta)
        {
            AssertDoublesAreEqual(expected, actual, delta, null, null);
        }

#if CLR_2_0 || CLR_4_0
        /// <summary>
        /// Verifies that two doubles are equal considering a delta. If the
        /// expected value is infinity then the delta value is ignored. If 
        /// they are not equal then an <see cref="AssertionException"/> is
        /// thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="delta">The maximum acceptable difference between the
        /// the expected and the actual</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void AreEqual(double expected, double? actual, double delta, string message, params object[] args)
        {
            AssertDoublesAreEqual(expected, (double)actual, delta, message, args);
        }
        /// <summary>
        /// Verifies that two doubles are equal considering a delta. If the
        /// expected value is infinity then the delta value is ignored. If 
        /// they are not equal then an <see cref="AssertionException"/> is
        /// thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="delta">The maximum acceptable difference between the
        /// the expected and the actual</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void AreEqual(double expected, double? actual, double delta, string message)
        {
            AssertDoublesAreEqual(expected, (double)actual, delta, message, null);
        }
        /// <summary>
        /// Verifies that two doubles are equal considering a delta. If the
        /// expected value is infinity then the delta value is ignored. If 
        /// they are not equal then an <see cref="AssertionException"/> is
        /// thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="delta">The maximum acceptable difference between the
        /// the expected and the actual</param>
        public static void AreEqual(double expected, double? actual, double delta)
        {
            AssertDoublesAreEqual(expected, (double)actual, delta, null, null);
        }
#endif

        #endregion

        #region Objects

        /// <summary>
        /// Verifies that two objects are equal.  Two objects are considered
        /// equal if both are null, or if both have the same value. NUnit
        /// has special semantics for some object types.
        /// If they are not equal an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The value that is expected</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void AreEqual(object expected, object actual, string message, params object[] args)
        {
            Assert.That(actual, Is.EqualTo(expected), message, args);
        }
        /// <summary>
        /// Verifies that two objects are equal.  Two objects are considered
        /// equal if both are null, or if both have the same value. NUnit
        /// has special semantics for some object types.
        /// If they are not equal an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The value that is expected</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void AreEqual(object expected, object actual, string message)
        {
            Assert.That(actual, Is.EqualTo(expected), message, null);
        }
        /// <summary>
        /// Verifies that two objects are equal.  Two objects are considered
        /// equal if both are null, or if both have the same value. NUnit
        /// has special semantics for some object types.
        /// If they are not equal an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The value that is expected</param>
        /// <param name="actual">The actual value</param>
        public static void AreEqual(object expected, object actual)
        {
            Assert.That(actual, Is.EqualTo(expected), null, null);
        }

        #endregion

        #endregion

        #region AreNotEqual

        #region Ints

        /// <summary>
        /// Verifies that two ints are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void AreNotEqual(int expected, int actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), message, args);
        }
        /// <summary>
        /// Verifies that two ints are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void AreNotEqual(int expected, int actual, string message)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), message, null);
        }
        /// <summary>
        /// Verifies that two ints are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        public static void AreNotEqual(int expected, int actual)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), null, null);
        }

        #endregion

        #region Longs

        /// <summary>
        /// Verifies that two longs are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void AreNotEqual(long expected, long actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), message, args);
        }
        /// <summary>
        /// Verifies that two longs are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void AreNotEqual(long expected, long actual, string message)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), message, null);
        }
        /// <summary>
        /// Verifies that two longs are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        public static void AreNotEqual(long expected, long actual)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), null, null);
        }

        #endregion

        #region Unsigned Ints

        /// <summary>
        /// Verifies that two unsigned ints are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        [CLSCompliant(false)]
        public static void AreNotEqual(uint expected, uint actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), message, args);
        }
        /// <summary>
        /// Verifies that two unsigned ints are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        [CLSCompliant(false)]
        public static void AreNotEqual(uint expected, uint actual, string message)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), message, null);
        }
        /// <summary>
        /// Verifies that two unsigned ints are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        [CLSCompliant(false)]
        public static void AreNotEqual(uint expected, uint actual)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), null, null);
        }

        #endregion

        #region Unsigned Longs

        /// <summary>
        /// Verifies that two unsigned longs are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        [CLSCompliant(false)]
        public static void AreNotEqual(ulong expected, ulong actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), message, args);
        }
        /// <summary>
        /// Verifies that two unsigned longs are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        [CLSCompliant(false)]
        public static void AreNotEqual(ulong expected, ulong actual, string message)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), message, null);
        }
        /// <summary>
        /// Verifies that two unsigned longs are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        [CLSCompliant(false)]
        public static void AreNotEqual(ulong expected, ulong actual)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), null, null);
        }

        #endregion

        #region Decimals

        /// <summary>
        /// Verifies that two decimals are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void AreNotEqual(decimal expected, decimal actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), message, args);
        }
        /// <summary>
        /// Verifies that two decimals are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void AreNotEqual(decimal expected, decimal actual, string message)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), message, null);
        }
        /// <summary>
        /// Verifies that two decimals are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        public static void AreNotEqual(decimal expected, decimal actual)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), null, null);
        }

        #endregion

        #region Floats

        /// <summary>
        /// Verifies that two floats are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void AreNotEqual(float expected, float actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), message, args);
        }
        /// <summary>
        /// Verifies that two floats are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void AreNotEqual(float expected, float actual, string message)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), message, null);
        }
        /// <summary>
        /// Verifies that two floats are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        public static void AreNotEqual(float expected, float actual)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), null, null);
        }

        #endregion

        #region Doubles

        /// <summary>
        /// Verifies that two doubles are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void AreNotEqual(double expected, double actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), message, args);
        }
        /// <summary>
        /// Verifies that two doubles are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void AreNotEqual(double expected, double actual, string message)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), message, null);
        }
        /// <summary>
        /// Verifies that two doubles are not equal. If they are equal, then an 
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        public static void AreNotEqual(double expected, double actual)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), null, null);
        }

        #endregion

        #region Objects

        /// <summary>
        /// Verifies that two objects are not equal.  Two objects are considered
        /// equal if both are null, or if both have the same value. NUnit
        /// has special semantics for some object types.
        /// If they are equal an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The value that is expected</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void AreNotEqual(object expected, object actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), message, args);
        }
        /// <summary>
        /// Verifies that two objects are not equal.  Two objects are considered
        /// equal if both are null, or if both have the same value. NUnit
        /// has special semantics for some object types.
        /// If they are equal an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The value that is expected</param>
        /// <param name="actual">The actual value</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void AreNotEqual(object expected, object actual, string message)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), message, null);
        }
        /// <summary>
        /// Verifies that two objects are not equal.  Two objects are considered
        /// equal if both are null, or if both have the same value. NUnit
        /// has special semantics for some object types.
        /// If they are equal an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The value that is expected</param>
        /// <param name="actual">The actual value</param>
        public static void AreNotEqual(object expected, object actual)
        {
            Assert.That(actual, Is.Not.EqualTo(expected), null, null);
        }

        #endregion

        #endregion

        #region AreSame

        /// <summary>
        /// Asserts that two objects refer to the same object. If they
        /// are not the same an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected object</param>
        /// <param name="actual">The actual object</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void AreSame(object expected, object actual, string message, params object[] args)
        {
            Assert.That(actual, Is.SameAs(expected), message, args);
        }
        /// <summary>
        /// Asserts that two objects refer to the same object. If they
        /// are not the same an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected object</param>
        /// <param name="actual">The actual object</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void AreSame(object expected, object actual, string message)
        {
            Assert.That(actual, Is.SameAs(expected), message, null);
        }
        /// <summary>
        /// Asserts that two objects refer to the same object. If they
        /// are not the same an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected object</param>
        /// <param name="actual">The actual object</param>
        public static void AreSame(object expected, object actual)
        {
            Assert.That(actual, Is.SameAs(expected), null, null);
        }

        #endregion

        #region AreNotSame

        /// <summary>
        /// Asserts that two objects do not refer to the same object. If they
        /// are the same an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected object</param>
        /// <param name="actual">The actual object</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void AreNotSame(object expected, object actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.SameAs(expected), message, args);
        }
        /// <summary>
        /// Asserts that two objects do not refer to the same object. If they
        /// are the same an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected object</param>
        /// <param name="actual">The actual object</param>
        /// <param name="message">The message to display in case of failure</param>
        public static void AreNotSame(object expected, object actual, string message)
        {
            Assert.That(actual, Is.Not.SameAs(expected), message, null);
        }
        /// <summary>
        /// Asserts that two objects do not refer to the same object. If they
        /// are the same an <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="expected">The expected object</param>
        /// <param name="actual">The actual object</param>
        public static void AreNotSame(object expected, object actual)
        {
            Assert.That(actual, Is.Not.SameAs(expected), null, null);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper for Assert.AreEqual(double expected, double actual, ...)
        /// allowing code generation to work consistently.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="delta">The maximum acceptable difference between the
        /// the expected and the actual</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        protected static void AssertDoublesAreEqual(double expected, double actual, double delta, string message, object[] args)
        {
            if (double.IsNaN(expected) || double.IsInfinity(expected))
                Assert.That(actual, Is.EqualTo(expected), message, args);
            else
                Assert.That(actual, Is.EqualTo(expected).Within(delta), message, args);
        }

        private static void IncrementAssertCount()
        {
            TestExecutionContext.CurrentContext.IncrementAssertCount();
        }

        #endregion
    }
}
