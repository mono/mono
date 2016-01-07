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

using System.Collections;
using NUnit.Framework.Internal;

namespace NUnit.Framework.Constraints
{
    /// <summary>
    /// Delegate used to delay evaluation of the actual value
    /// to be used in evaluating a constraint
    /// </summary>
#if CLR_2_0 || CLR_4_0
    public delegate T ActualValueDelegate<T>();
#else
    public delegate object ActualValueDelegate();
#endif

    /// <summary>
    /// The Constraint class is the base of all built-in constraints
    /// within NUnit. It provides the operator overloads used to combine 
    /// constraints.
    /// </summary>
    public abstract class Constraint : IResolveConstraint
    {
        #region UnsetObject Class
        /// <summary>
        /// Class used to detect any derived constraints
        /// that fail to set the actual value in their
        /// Matches override.
        /// </summary>
        private class UnsetObject
        {
            public override string ToString()
            {
                return "UNSET";
            }
        }
        #endregion

        #region Static and Instance Fields
        /// <summary>
        /// Static UnsetObject used to detect derived constraints
        /// failing to set the actual value.
        /// </summary>
        protected static object UNSET = new UnsetObject();

        /// <summary>
        /// The actual value being tested against a constraint
        /// </summary>
        protected object actual = UNSET;

        /// <summary>
        /// The display name of this Constraint for use by ToString()
        /// </summary>
        private string displayName;

        /// <summary>
        /// Argument fields used by ToString();
        /// </summary>
        private readonly int argcnt;
        private readonly object arg1;
        private readonly object arg2;

        /// <summary>
        /// The builder holding this constraint
        /// </summary>
        private ConstraintBuilder builder;
        #endregion

        #region Constructors
        /// <summary>
        /// Construct a constraint with no arguments
        /// </summary>
        protected Constraint()
        {
            argcnt = 0;
        }

        /// <summary>
        /// Construct a constraint with one argument
        /// </summary>
        protected Constraint(object arg)
        {
            argcnt = 1;
            this.arg1 = arg;
        }

        /// <summary>
        /// Construct a constraint with two arguments
        /// </summary>
        protected Constraint(object arg1, object arg2)
        {
            argcnt = 2;
            this.arg1 = arg1;
            this.arg2 = arg2;
        }
        #endregion

        #region Set Containing ConstraintBuilder
        /// <summary>
        /// Sets the ConstraintBuilder holding this constraint
        /// </summary>
        internal void SetBuilder(ConstraintBuilder builder)
        {
            this.builder = builder;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The display name of this Constraint for use by ToString().
        /// The default value is the name of the constraint with
        /// trailing "Constraint" removed. Derived classes may set
        /// this to another name in their constructors.
        /// </summary>
        protected string DisplayName
        {
            get
            {
                if (displayName == null)
                {
                    displayName = this.GetType().Name.ToLower();
                    if (displayName.EndsWith("`1") || displayName.EndsWith("`2"))
                        displayName = displayName.Substring(0, displayName.Length - 2);
                    if (displayName.EndsWith("constraint"))
                        displayName = displayName.Substring(0, displayName.Length - 10);
                }

                return displayName;
            }

            set { displayName = value; }
        }
        #endregion

        #region Abstract and Virtual Methods
        /// <summary>
        /// Write the failure message to the MessageWriter provided
        /// as an argument. The default implementation simply passes
        /// the constraint and the actual value to the writer, which
        /// then displays the constraint description and the value.
        /// 
        /// Constraints that need to provide additional details,
        /// such as where the error occured can override this.
        /// </summary>
        /// <param name="writer">The MessageWriter on which to display the message</param>
        public virtual void WriteMessageTo(MessageWriter writer)
        {
            writer.DisplayDifferences(this);
        }

        /// <summary>
        /// Test whether the constraint is satisfied by a given value
        /// </summary>
        /// <param name="actual">The value to be tested</param>
        /// <returns>True for success, false for failure</returns>
        public abstract bool Matches(object actual);

#if CLR_2_0 || CLR_4_0
        /// <summary>
        /// Test whether the constraint is satisfied by an
        /// ActualValueDelegate that returns the value to be tested.
        /// The default implementation simply evaluates the delegate
        /// but derived classes may override it to provide for delayed 
        /// processing.
        /// </summary>
        /// <param name="del">An <see cref="ActualValueDelegate{T}" /></param>
        /// <returns>True for success, false for failure</returns>
        public virtual bool Matches<T>(ActualValueDelegate<T> del)
        {
#if NET_4_5
            if (AsyncInvocationRegion.IsAsyncOperation(del))
                using (var region = AsyncInvocationRegion.Create(del))
                    return Matches(region.WaitForPendingOperationsToComplete(del()));
#endif
            return Matches(del());
        }
#else
        /// <summary>
        /// Test whether the constraint is satisfied by an
        /// ActualValueDelegate that returns the value to be tested.
        /// The default implementation simply evaluates the delegate
        /// but derived classes may override it to provide for delayed 
        /// processing.
        /// </summary>
        /// <param name="del">An <see cref="ActualValueDelegate" /></param>
        /// <returns>True for success, false for failure</returns>
        public virtual bool Matches(ActualValueDelegate del)
        {
            return Matches(del());
        }
#endif

        /// <summary>
        /// Test whether the constraint is satisfied by a given reference.
        /// The default implementation simply dereferences the value but
        /// derived classes may override it to provide for delayed processing.
        /// </summary>
        /// <param name="actual">A reference to the value to be tested</param>
        /// <returns>True for success, false for failure</returns>
#if CLR_2_0 || CLR_4_0
        public virtual bool Matches<T>(ref T actual)
#else
        public virtual bool Matches(ref bool actual)
#endif
        {
            return Matches(actual);
        }

        /// <summary>
        /// Write the constraint description to a MessageWriter
        /// </summary>
        /// <param name="writer">The writer on which the description is displayed</param>
        public abstract void WriteDescriptionTo(MessageWriter writer);

        /// <summary>
        /// Write the actual value for a failing constraint test to a
        /// MessageWriter. The default implementation simply writes
        /// the raw value of actual, leaving it to the writer to
        /// perform any formatting.
        /// </summary>
        /// <param name="writer">The writer on which the actual value is displayed</param>
        public virtual void WriteActualValueTo(MessageWriter writer)
        {
            writer.WriteActualValue(actual);
        }
        #endregion

        #region ToString Override
        /// <summary>
        /// Default override of ToString returns the constraint DisplayName
        /// followed by any arguments within angle brackets.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string rep = GetStringRepresentation();

            return this.builder == null ? rep : string.Format("<unresolved {0}>", rep);
        }

        /// <summary>
        /// Returns the string representation of this constraint
        /// </summary>
        protected virtual string GetStringRepresentation()
        {
            switch (argcnt)
            {
                default:
                case 0:
                    return string.Format("<{0}>", DisplayName);
                case 1:
                    return string.Format("<{0} {1}>", DisplayName, _displayable(arg1));
                case 2:
                    return string.Format("<{0} {1} {2}>", DisplayName, _displayable(arg1), _displayable(arg2));
            }
        }

        private static string _displayable(object o)
        {
            if (o == null) return "null";

            string fmt = o is string ? "\"{0}\"" : "{0}";
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, fmt, o);
        }
        #endregion

        #region Operator Overloads
        /// <summary>
        /// This operator creates a constraint that is satisfied only if both 
        /// argument constraints are satisfied.
        /// </summary>
        public static Constraint operator &(Constraint left, Constraint right)
        {
            IResolveConstraint l = (IResolveConstraint)left;
            IResolveConstraint r = (IResolveConstraint)right;
            return new AndConstraint(l.Resolve(), r.Resolve());
        }

        /// <summary>
        /// This operator creates a constraint that is satisfied if either 
        /// of the argument constraints is satisfied.
        /// </summary>
        public static Constraint operator |(Constraint left, Constraint right)
        {
            IResolveConstraint l = (IResolveConstraint)left;
            IResolveConstraint r = (IResolveConstraint)right;
            return new OrConstraint(l.Resolve(), r.Resolve());
        }

        /// <summary>
        /// This operator creates a constraint that is satisfied if the 
        /// argument constraint is not satisfied.
        /// </summary>
        public static Constraint operator !(Constraint constraint)
        {
            IResolveConstraint r = constraint as IResolveConstraint;
            return new NotConstraint(r == null ? new NullConstraint() : r.Resolve());
        }
        #endregion

        #region Binary Operators
        /// <summary>
        /// Returns a ConstraintExpression by appending And
        /// to the current constraint.
        /// </summary>
        public ConstraintExpression And
        {
            get
            {
                ConstraintBuilder builder = this.builder;
                if (builder == null)
                {
                    builder = new ConstraintBuilder();
                    builder.Append(this);
                }

                builder.Append(new AndOperator());

                return new ConstraintExpression(builder);
            }
        }

        /// <summary>
        /// Returns a ConstraintExpression by appending And
        /// to the current constraint.
        /// </summary>
        public ConstraintExpression With
        {
            get { return this.And; }
        }

        /// <summary>
        /// Returns a ConstraintExpression by appending Or
        /// to the current constraint.
        /// </summary>
        public ConstraintExpression Or
        {
            get
            {
                ConstraintBuilder builder = this.builder;
                if (builder == null)
                {
                    builder = new ConstraintBuilder();
                    builder.Append(this);
                }

                builder.Append(new OrOperator());

                return new ConstraintExpression(builder);
            }
        }
        #endregion

        #region After Modifier

#if !SILVERLIGHT
        /// <summary>
        /// Returns a DelayedConstraint with the specified delay time.
        /// </summary>
        /// <param name="delayInMilliseconds">The delay in milliseconds.</param>
        /// <returns></returns>
        public DelayedConstraint After(int delayInMilliseconds)
        {
            return new DelayedConstraint(
                builder == null ? this : builder.Resolve(),
                delayInMilliseconds);
        }

        /// <summary>
        /// Returns a DelayedConstraint with the specified delay time
        /// and polling interval.
        /// </summary>
        /// <param name="delayInMilliseconds">The delay in milliseconds.</param>
        /// <param name="pollingInterval">The interval at which to test the constraint.</param>
        /// <returns></returns>
        public DelayedConstraint After(int delayInMilliseconds, int pollingInterval)
        {
            return new DelayedConstraint(
                builder == null ? this : builder.Resolve(),
                delayInMilliseconds,
                pollingInterval);
        }
#endif

        #endregion

        #region IResolveConstraint Members
        Constraint IResolveConstraint.Resolve()
        {
            return builder == null ? this : builder.Resolve();
        }
        #endregion
    }
}