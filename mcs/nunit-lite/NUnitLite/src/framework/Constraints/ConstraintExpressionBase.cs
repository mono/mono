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
using System.Collections;

namespace NUnit.Framework.Constraints
{
    /// <summary>
    /// ConstraintExpressionBase is the abstract base class for the 
    /// ConstraintExpression class, which represents a 
    /// compound constraint in the process of being constructed 
    /// from a series of syntactic elements.
    /// 
    /// NOTE: ConstraintExpressionBase is separate because the
    /// ConstraintExpression class was generated in earlier
    /// versions of NUnit. The two classes may be combined
    /// in a future version.
    /// </summary>
    public abstract class ConstraintExpressionBase
    {
        #region Instance Fields
        /// <summary>
        /// The ConstraintBuilder holding the elements recognized so far
        /// </summary>
        protected ConstraintBuilder builder;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ConstraintExpressionBase"/> class.
        /// </summary>
        public ConstraintExpressionBase()
        {
            this.builder = new ConstraintBuilder();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ConstraintExpressionBase"/> 
        /// class passing in a ConstraintBuilder, which may be pre-populated.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public ConstraintExpressionBase(ConstraintBuilder builder)
        {
            this.builder = builder;
        }
        #endregion

        #region ToString()
        /// <summary>
        /// Returns a string representation of the expression as it
        /// currently stands. This should only be used for testing,
        /// since it has the side-effect of resolving the expression.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return builder.Resolve().ToString();
        }
        #endregion

        #region Append Methods
        /// <summary>
        /// Appends an operator to the expression and returns the
        /// resulting expression itself.
        /// </summary>
        public ConstraintExpression Append(ConstraintOperator op)
        {
            builder.Append(op);
            return (ConstraintExpression)this;
        }

        /// <summary>
        /// Appends a self-resolving operator to the expression and
        /// returns a new ResolvableConstraintExpression.
        /// </summary>
        public ResolvableConstraintExpression Append(SelfResolvingOperator op)
        {
            builder.Append(op);
            return new ResolvableConstraintExpression(builder);
        }

        /// <summary>
        /// Appends a constraint to the expression and returns that
        /// constraint, which is associated with the current state
        /// of the expression being built.
        /// </summary>
        public Constraint Append(Constraint constraint)
        {
            builder.Append(constraint);
            return constraint;
        }
        #endregion
    }
}
