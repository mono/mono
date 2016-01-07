// ***********************************************************************
// Copyright (c) 2008 Charlie Poole
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

namespace NUnit.Framework.Constraints
{
    /// <summary>
    /// ThrowsConstraint is used to test the exception thrown by 
    /// a delegate by applying a constraint to it.
    /// </summary>
    public class ThrowsConstraint : PrefixConstraint
    {
        private Exception caughtException;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrowsConstraint"/> class,
        /// using a constraint to be applied to the exception.
        /// </summary>
        /// <param name="baseConstraint">A constraint to apply to the caught exception.</param>
        public ThrowsConstraint(Constraint baseConstraint)
            : base(baseConstraint) { }

        /// <summary>
        /// Get the actual exception thrown - used by Assert.Throws.
        /// </summary>
        public Exception ActualException
        {
            get { return caughtException; }
        }

        #region Constraint Overrides

        /// <summary>
        /// Executes the code of the delegate and captures any exception.
        /// If a non-null base constraint was provided, it applies that
        /// constraint to the exception.
        /// </summary>
        /// <param name="actual">A delegate representing the code to be tested</param>
        /// <returns>True if an exception is thrown and the constraint succeeds, otherwise false</returns>
        public override bool Matches(object actual)
        {
            caughtException = ExceptionInterceptor.Intercept(actual);

            if (caughtException == null)
                return false;

            return baseConstraint == null || baseConstraint.Matches(caughtException);
        }

        /// <summary>
        /// Converts an ActualValueDelegate to a TestDelegate
        /// before calling the primary overload.
        /// </summary>
#if CLR_2_0 || CLR_4_0
        public override bool Matches<T>(ActualValueDelegate<T> del)
        {
            return Matches(new GenericInvocationDescriptor<T>(del));
        }
#else
        public override bool Matches(ActualValueDelegate del)
        {
            return Matches(new ObjectInvocationDescriptor(del));
        }
#endif

        /// <summary>
        /// Write the constraint description to a MessageWriter
        /// </summary>
        /// <param name="writer">The writer on which the description is displayed</param>
        public override void WriteDescriptionTo(MessageWriter writer)
        {
            if (baseConstraint == null)
                writer.WritePredicate("an exception");
            else
                baseConstraint.WriteDescriptionTo(writer);
        }

        /// <summary>
        /// Write the actual value for a failing constraint test to a
        /// MessageWriter. The default implementation simply writes
        /// the raw value of actual, leaving it to the writer to
        /// perform any formatting.
        /// </summary>
        /// <param name="writer">The writer on which the actual value is displayed</param>
        public override void WriteActualValueTo(MessageWriter writer)
        {
            if (caughtException == null)
                writer.Write("no exception thrown");
            else if (baseConstraint != null)
                baseConstraint.WriteActualValueTo(writer);
            else
                writer.WriteActualValue(caughtException);
        }
        #endregion

        /// <summary>
        /// Returns the string representation of this constraint
        /// </summary>
        protected override string GetStringRepresentation()
        {
            if (baseConstraint == null)
                return "<throws>";

            return base.GetStringRepresentation();
        }
    }

    #region ExceptionInterceptor

    internal class ExceptionInterceptor
    {
        private ExceptionInterceptor() { }

        internal static Exception Intercept(object invocation)
        {
            IInvocationDescriptor invocationDescriptor = GetInvocationDescriptor(invocation);

#if NET_4_5
            if (AsyncInvocationRegion.IsAsyncOperation(invocationDescriptor.Delegate))
            {
                using (AsyncInvocationRegion region = AsyncInvocationRegion.Create(invocationDescriptor.Delegate))
                {
                    object result = invocationDescriptor.Invoke();

                    try
                    {
                        region.WaitForPendingOperationsToComplete(result);
                        return null;
                    }
                    catch (Exception ex)
                    {
                        return ex;
                    }
                }
            }
            else
#endif
            {
                try
                {
                    invocationDescriptor.Invoke();
                    return null;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }
        }

        private static IInvocationDescriptor GetInvocationDescriptor(object actual)
        {
            IInvocationDescriptor invocationDescriptor = actual as IInvocationDescriptor;

            if (invocationDescriptor == null)
            {
                TestDelegate testDelegate = actual as TestDelegate;

                if (testDelegate == null)
                    throw new ArgumentException(
                        String.Format("The actual value must be a TestDelegate or ActualValueDelegate but was {0}", actual.GetType().Name),
                        "actual");

                invocationDescriptor = new VoidInvocationDescriptor(testDelegate);
            }

            return invocationDescriptor;
        }
    }

    #endregion

    #region InvocationDescriptor

    internal class VoidInvocationDescriptor : IInvocationDescriptor
    {
        private readonly TestDelegate _del;

        public VoidInvocationDescriptor(TestDelegate del)
        {
            _del = del;
        }

        public object Invoke()
        {
            _del();
            return null;
        }

        public Delegate Delegate
        {
            get { return _del; }
        }
    }

#if CLR_2_0 || CLR_4_0
    internal class GenericInvocationDescriptor<T> : IInvocationDescriptor
    {
        private readonly ActualValueDelegate<T> _del;

        public GenericInvocationDescriptor(ActualValueDelegate<T> del)
        {
            _del = del;
        }

        public object Invoke()
        {
            return _del();
        }

        public Delegate Delegate
        {
            get { return _del; }
        }
    }
#else
	internal class ObjectInvocationDescriptor : IInvocationDescriptor
	{
		private readonly ActualValueDelegate _del;

		public ObjectInvocationDescriptor(ActualValueDelegate del)
		{
			_del = del;
		}

		public object Invoke()
		{
			return _del();
		}

		public Delegate Delegate
		{
			get { return _del; }
		}
	}
#endif

    internal interface IInvocationDescriptor
    {
        object Invoke();
        Delegate Delegate { get; }
    }

    #endregion
}