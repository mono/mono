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

namespace NUnit.Framework.Constraints
{
    /// <summary>
    /// The ConstraintOperator class is used internally by a
    /// ConstraintBuilder to represent an operator that 
    /// modifies or combines constraints. 
    /// 
    /// Constraint operators use left and right precedence
    /// values to determine whether the top operator on the
    /// stack should be reduced before pushing a new operator.
    /// </summary>
    public abstract class ConstraintOperator
    {
        private object leftContext;
        private object rightContext;

        /// <summary>
        /// The precedence value used when the operator
        /// is about to be pushed to the stack.
        /// </summary>
        protected int left_precedence;

        /// <summary>
        /// The precedence value used when the operator
        /// is on the top of the stack.
        /// </summary>
        protected int right_precedence;

        /// <summary>
        /// The syntax element preceding this operator
        /// </summary>
        public object LeftContext
        {
            get { return leftContext; }
            set { leftContext = value; }
        }

        /// <summary>
        /// The syntax element folowing this operator
        /// </summary>
        public object RightContext
        {
            get { return rightContext; }
            set { rightContext = value; }
        }

        /// <summary>
        /// The precedence value used when the operator
        /// is about to be pushed to the stack.
        /// </summary>
        public virtual int LeftPrecedence
        {
            get { return left_precedence; }
        }

        /// <summary>
        /// The precedence value used when the operator
        /// is on the top of the stack.
        /// </summary>
        public virtual int RightPrecedence
        {
            get { return right_precedence; }
        }

        /// <summary>
        /// Reduce produces a constraint from the operator and 
        /// any arguments. It takes the arguments from the constraint 
        /// stack and pushes the resulting constraint on it.
        /// </summary>
        /// <param name="stack"></param>
        public abstract void Reduce(ConstraintBuilder.ConstraintStack stack);
    }
}