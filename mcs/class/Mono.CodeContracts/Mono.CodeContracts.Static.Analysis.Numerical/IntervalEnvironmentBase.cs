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

using System;
using System.Collections.Generic;
using System.IO;

using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    abstract class IntervalEnvironmentBase<TEnv, TVar, TExpr, TInterval, TNumeric> : IAbstractDomainForEnvironments<TEnv, TVar, TExpr> 
        where TEnv : IntervalEnvironmentBase<TEnv, TVar, TExpr, TInterval, TNumeric>
        where TInterval : IntervalBase<TInterval, TNumeric> 
        where TVar : IEquatable<TVar> 
    {
        public readonly IntervalAssumerBase<TEnv, TVar, TExpr, TInterval, TNumeric> Assumer;
        public static readonly IIntervalHelper<TInterval,TNumeric> IntervalHelper;

        private readonly EnvironmentDomain<TVar, TInterval> varsToIntervals;
        
        private IExpressionDecoder<TVar, TExpr> decoder;

        public abstract TNumeric PlusInfinity { get; }
        public abstract TNumeric MinusInfinity { get; }

        public abstract TInterval IntervalUnknown { get; }

        public IEnumerable<TVar> Variables { get { return this.varsToIntervals.Keys; } }

        protected IntervalEnvironmentBase(IntervalEnvironmentBase<TEnv, TVar, TExpr, TInterval, TNumeric> original, IntervalAssumerBase<TEnv, TVar, TExpr, TInterval, TNumeric> assumer)
        {
            this.Assumer = assumer;
            this.varsToIntervals = EnvironmentDomain<TVar, TInterval>.TopValue (null);
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

        public TEnv TestTrueEqual(TExpr left, TExpr right)
        {
            var leftVar = decoder.UnderlyingVariable (left);
            var rightVar = decoder.UnderlyingVariable (right);

            if (this.varsToIntervals.Contains(leftVar))
            {
                var res = varsToIntervals;
                var interval = Eval(left).Meet(Eval(right));

                res = res.With (leftVar, interval);
                res = res.With (rightVar, interval);

                return NewInstance (res);
            }
            if (decoder.IsConstant(left) && decoder.IsConstant(right) && this.Eval(left).Meet(Eval(right)).IsBottom)
                return Bottom;

            return (TEnv)this;
        }
        
        public TInterval Eval (TExpr expr)
        {
            int intValue;
            if (decoder.IsConstantInt(expr, out intValue))
                return IntervalHelper.For (intValue);

            throw new NotImplementedException ();
        }

        protected abstract bool IsZero (TNumeric lowerBound);

        protected abstract bool IsGreaterThanZero (TNumeric lowerBound);
        protected abstract bool IsGreaterEqualThanZero (TNumeric lowerBound);

        protected abstract bool IsLessThanZero (TNumeric upperBound);
        protected abstract bool IsLessEqualThanZero (TNumeric upperBound);

        public string ToString(TExpr expr)
        {
            // TODO: implement this
            return "< not implemented >";
        }

        public TEnv TestTrue (TExpr guard)
        {
            this.TestNotEqualToZero (decoder.UnderlyingVariable (guard));
            return new IntervalTestVisitor (decoder).VisitTrue (guard, this as TEnv);
        }

        protected abstract TEnv TestNotEqualToZero (TVar var);

        public TEnv TestFalse (TExpr guard)
        {
            throw new System.NotImplementedException ();
        }

        public FlatDomain<bool> CheckIfHolds (TExpr expr)
        {
            throw new System.NotImplementedException ();
        }

        public bool TryGetValue(TVar v, out TInterval interval)
        {
            return this.varsToIntervals.TryGetValue (v, out interval);
        }

        public TEnv With(TVar var, TInterval interval)
        {
            return NewInstance (varsToIntervals.With (var, interval));
        }

        public TEnv RefineVariable(TVar var, TInterval interval)
        {
            TInterval current;
            if (TryGetValue(var, out current))
                interval = interval.Meet(current);

            return this.With(var, interval);
        }

        #region Implementation of IAbstractDomain<T>

        public TEnv Top { get { return NewInstance (EnvironmentDomain<TVar, TInterval>.TopValue (null)); } }

        public TEnv Bottom { get { return NewInstance (EnvironmentDomain<TVar, TInterval>.BottomValue); } }

        public bool IsTop { get { return this.varsToIntervals.IsTop; } }

        public bool IsBottom { get { return this.varsToIntervals.IsBottom; } }

        public TEnv Join (TEnv that)
        {
            return NewInstance (this.varsToIntervals.Join (that.varsToIntervals));
        }

        public TEnv Join (TEnv that, bool widen, out bool weaker)
        {
            return NewInstance (this.varsToIntervals.Join (that.varsToIntervals, widen, out weaker));
        }

        public TEnv Widen (TEnv that)
        {
            return NewInstance (this.varsToIntervals.Widen (that.varsToIntervals));
        }

        public TEnv Meet (TEnv that)
        {
            return NewInstance (this.varsToIntervals.Meet (that.varsToIntervals));
        }

        public bool LessEqual (TEnv that)
        {
            return this.varsToIntervals.LessEqual (that.varsToIntervals);
        }

        public TEnv ImmutableVersion ()
        {
            return this as TEnv;
        }

        public TEnv Clone ()
        {
            return this as TEnv;
        }

        public void Dump (TextWriter tw)
        {
            this.varsToIntervals.Dump (tw);
        }

        #endregion

        protected abstract TEnv NewInstance(EnvironmentDomain<TVar, TInterval> varsToIntervals);


        private class IntervalTestVisitor
        {
            private readonly IntervalTestTrueVisitor<TEnv, TVar, TExpr, TInterval, TNumeric> trueVisitor;
            private readonly IntervalTestFalseVisitor<TEnv, TVar, TExpr, TInterval, TNumeric> falseVisitor;

            public IntervalTestVisitor(IExpressionDecoder<TVar, TExpr> decoder)
            {
                this.trueVisitor = new IntervalTestTrueVisitor<TEnv, TVar, TExpr, TInterval, TNumeric>(decoder);
                this.falseVisitor = new IntervalTestFalseVisitor<TEnv, TVar, TExpr, TInterval, TNumeric>(decoder);

                this.trueVisitor.FalseVisitor = this.falseVisitor;
                this.falseVisitor.TrueVisitor = this.trueVisitor;
            }

            public TEnv VisitTrue(TExpr guard, TEnv data)
            {
                return trueVisitor.Visit(guard, data);
            }

            public TEnv VisitFalse(TExpr guard, TEnv data)
            {
                return falseVisitor.Visit(guard, data);
            }
        }
    }
}