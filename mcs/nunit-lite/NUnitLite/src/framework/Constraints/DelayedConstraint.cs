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
using System.Threading;
using NUnit.Framework.Internal;

namespace NUnit.Framework.Constraints
{
    ///<summary>
    /// Applies a delay to the match so that a match can be evaluated in the future.
    ///</summary>
    public class DelayedConstraint : PrefixConstraint
    {
        private readonly int delayInMilliseconds;
        private readonly int pollingInterval;

        ///<summary>
        /// Creates a new DelayedConstraint
        ///</summary>
        ///<param name="baseConstraint">The inner constraint two decorate</param>
        ///<param name="delayInMilliseconds">The time interval after which the match is performed</param>
        ///<exception cref="InvalidOperationException">If the value of <paramref name="delayInMilliseconds"/> is less than 0</exception>
        public DelayedConstraint(Constraint baseConstraint, int delayInMilliseconds)
            : this(baseConstraint, delayInMilliseconds, 0) { }

        ///<summary>
        /// Creates a new DelayedConstraint
        ///</summary>
        ///<param name="baseConstraint">The inner constraint two decorate</param>
        ///<param name="delayInMilliseconds">The time interval after which the match is performed</param>
        ///<param name="pollingInterval">The time interval used for polling</param>
        ///<exception cref="InvalidOperationException">If the value of <paramref name="delayInMilliseconds"/> is less than 0</exception>
        public DelayedConstraint(Constraint baseConstraint, int delayInMilliseconds, int pollingInterval)
            : base(baseConstraint)
        {
            if (delayInMilliseconds < 0)
                throw new ArgumentException("Cannot check a condition in the past", "delayInMilliseconds");

            this.delayInMilliseconds = delayInMilliseconds;
            this.pollingInterval = pollingInterval;
        }

        /// <summary>
        /// Test whether the constraint is satisfied by a given value
        /// </summary>
        /// <param name="actual">The value to be tested</param>
        /// <returns>True for if the base constraint fails, false if it succeeds</returns>
        public override bool Matches(object actual)
        {
            int remainingDelay = delayInMilliseconds;

            while (pollingInterval > 0 && pollingInterval < remainingDelay)
            {
                remainingDelay -= pollingInterval;
                Thread.Sleep(pollingInterval);
                this.actual = actual;
                if (baseConstraint.Matches(actual))
                    return true;
            }

            if (remainingDelay > 0)
                Thread.Sleep(remainingDelay);
            this.actual = actual;
            return baseConstraint.Matches(actual);
        }

        /// <summary>
        /// Test whether the constraint is satisfied by a delegate
        /// </summary>
        /// <param name="del">The delegate whose value is to be tested</param>
        /// <returns>True for if the base constraint fails, false if it succeeds</returns>
#if CLR_2_0 || CLR_4_0
        public override bool Matches<T>(ActualValueDelegate<T> del)
#else
        public override bool Matches(ActualValueDelegate del)
#endif
        {
            int remainingDelay = delayInMilliseconds;

            while (pollingInterval > 0 && pollingInterval < remainingDelay)
            {
                remainingDelay -= pollingInterval;
                Thread.Sleep(pollingInterval);
                this.actual = InvokeDelegate(del);
				
				try
				{
	                if (baseConstraint.Matches(actual))
	                    return true;
				}
				catch
				{
					// Ignore any exceptions when polling
				}
            }

            if (remainingDelay > 0)
                Thread.Sleep(remainingDelay);
            this.actual = InvokeDelegate(del);
            return baseConstraint.Matches(actual);
        }

#if CLR_2_0 || CLR_4_0
        private static object InvokeDelegate<T>(ActualValueDelegate<T> del)
        {
#if NET_4_5
            if (AsyncInvocationRegion.IsAsyncOperation(del))
                using (AsyncInvocationRegion region = AsyncInvocationRegion.Create(del))
                    return region.WaitForPendingOperationsToComplete(del());
#endif

            return del();
        }
#else
        private static object InvokeDelegate(ActualValueDelegate del)
        {
            return del();
        }
#endif

#if CLR_2_0 || CLR_4_0
        /// <summary>
        /// Test whether the constraint is satisfied by a given reference.
        /// Overridden to wait for the specified delay period before
        /// calling the base constraint with the dereferenced value.
        /// </summary>
        /// <param name="actual">A reference to the value to be tested</param>
        /// <returns>True for success, false for failure</returns>
        public override bool Matches<T>(ref T actual)
        {
            int remainingDelay = delayInMilliseconds;

            while (pollingInterval > 0 && pollingInterval < remainingDelay)
            {
                remainingDelay -= pollingInterval;
                Thread.Sleep(pollingInterval);
                this.actual = actual;

                try
                {
                    if (baseConstraint.Matches(actual))
                        return true;
                }
                catch (Exception)
                {
                    // Ignore any exceptions when polling
                }
            }

            if (remainingDelay > 0)
                Thread.Sleep(remainingDelay);
            this.actual = actual;
            return baseConstraint.Matches(actual);
        }
#else
        /// <summary>
        /// Test whether the constraint is satisfied by a given boolean reference.
        /// Overridden to wait for the specified delay period before
        /// calling the base constraint with the dereferenced value.
        /// </summary>
        /// <param name="actual">A reference to the value to be tested</param>
        /// <returns>True for success, false for failure</returns>
        public override bool Matches(ref bool actual)
        {
            int remainingDelay = delayInMilliseconds;

            while (pollingInterval > 0 && pollingInterval < remainingDelay)
            {
                remainingDelay -= pollingInterval;
                Thread.Sleep(pollingInterval);
                this.actual = actual;
				
                if (baseConstraint.Matches(actual))
                    return true;
            }

            if (remainingDelay > 0)
                Thread.Sleep(remainingDelay);
            this.actual = actual;
            return baseConstraint.Matches(actual);
        }
#endif

        /// <summary>
        /// Write the constraint description to a MessageWriter
        /// </summary>
        /// <param name="writer">The writer on which the description is displayed</param>
        public override void WriteDescriptionTo(MessageWriter writer)
        {
            baseConstraint.WriteDescriptionTo(writer);
            writer.Write(string.Format(" after {0} millisecond delay", delayInMilliseconds));
        }

        /// <summary>
        /// Write the actual value for a failing constraint test to a MessageWriter.
        /// </summary>
        /// <param name="writer">The writer on which the actual value is displayed</param>
        public override void WriteActualValueTo(MessageWriter writer)
        {
            baseConstraint.WriteActualValueTo(writer);
        }

        /// <summary>
        /// Returns the string representation of the constraint.
        /// </summary>
        protected override string GetStringRepresentation()
        {
            return string.Format("<after {0} {1}>", delayInMilliseconds, baseConstraint);
        }
    }
}
