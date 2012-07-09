// 
// IntervalEnvironmentBase.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2012 Alexander Chebaturkin
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
// 

using System.Collections.Generic;

using Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Decoding;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    abstract class IntervalEnvironmentBase<TEnv, Var, Expr, TInterval, TNumeric> : FunctionalAbstractDomain<TEnv, Var, TInterval>, IAbstractDomainForEnvironments<TEnv, Var, Expr> 
        where TEnv : IntervalEnvironmentBase<TEnv, Var, Expr, TInterval, TNumeric>
        where TInterval : IntervalBase<TInterval, TNumeric>
    {
        private IExpressionDecoder<Var, Expr> decoder;

        public abstract TNumeric PlusInfinity { get; }
        public abstract TNumeric MinusInfinity { get; }

        public abstract TInterval IntervalUnknown { get; }

        public IEnumerable<Var> Variables
        {
            get { return this.Keys; }
        }

        protected IntervalEnvironmentBase(IntervalEnvironmentBase<TEnv, Var, Expr, TInterval, TNumeric> original)
            : base(original)
        {
        }

        public abstract TInterval For (long v);
        public abstract TInterval For (TNumeric lower, TNumeric upper);

        public virtual FlatDomain<bool> IsNotZero(TInterval intv)
        {
            if (intv.IsNormal ())
            {
                if (this.IsGreaterThanZero(intv.LowerBound) || this.IsLessThanZero(intv.UpperBound))
                    return true;
                if (this.IsZero(intv.LowerBound) && this.IsZero(intv.UpperBound))
                    return false;
            }
            
            return FlatDomain<bool>.TopValue;
        }

        public bool IsGreaterEqualThanZero(TInterval intv)
        {
            if (intv.IsNormal())
                return this.IsGreaterEqualThanZero (intv.LowerBound);

            return false;
        }

        public bool IsLessEqualThanZero(TInterval intv)
        {
            if (intv.IsNormal())
                return this.IsLessEqualThanZero(intv.UpperBound);

            return false;
        }

        public void Assign(Expr x, Expr y)
        {
            this.State = AbstractState.Normal;

            TInterval intv = this.Eval (y);
            Var var = this.decoder.UnderlyingVariable (x);
            if (intv.IsBottom)
            {
                this.State = AbstractState.Bottom;
                this.ClearElements();
            }
            else
            {
                if (intv.IsTop)
                    return;

                TInterval was;
                if (base.TryGetValue (var, out was))
                    intv = intv.Meet (was);
                this[var] = intv;
            }
        }



        protected abstract TInterval Eval (Expr expr);

        protected abstract bool IsZero (TNumeric lowerBound);

        protected abstract bool IsGreaterThanZero (TNumeric lowerBound);
        protected abstract bool IsGreaterEqualThanZero (TNumeric lowerBound);

        protected abstract bool IsLessThanZero (TNumeric upperBound);
        protected abstract bool IsLessEqualThanZero (TNumeric upperBound);

        public string ToString(Expr expr)
        {
            // TODO: implement this
            return "< not implemented >";
        }

        private class IntervalTrueTestVisitor : TestTrueVisitor<TEnv, Var, Expr>
        {
            public IntervalTrueTestVisitor (IExpressionDecoder<Var, Expr> decoder)
                : base (decoder)
            {
            }

            public override TEnv VisitLessEqualThan(Expr left, Expr right, Expr original, TEnv data)
            {
                return data;
                //TODO:
//                return data.TestTrueLessEqualThan (left, right);
            }
        }
    }
}