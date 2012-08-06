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

using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    abstract class IntervalEnvironmentBase<TEnv, TVar, TExpr, TInterval, TNumeric> : IIntervalEnvironment<TVar, TExpr, TInterval, TNumeric>,IAbstractDomainForEnvironments<TEnv, TVar, TExpr> 
        where TEnv : IntervalEnvironmentBase<TEnv, TVar, TExpr, TInterval, TNumeric>
        where TInterval : IntervalBase<TInterval, TNumeric> 
        where TVar : IEquatable<TVar> 
    {
        public abstract IntervalAssumerBase<TEnv, TVar, TExpr, TInterval, TNumeric> Assumer { get; }
        public abstract IntervalContextBase<TInterval, TNumeric> Context { get; }

        public readonly IExpressionDecoder<TVar, TExpr> Decoder;
        private readonly EnvironmentDomain<TVar, TInterval> varsToIntervals;

        public IEnumerable<TVar> Variables { get { return this.varsToIntervals.Keys; } }

        protected IntervalEnvironmentBase(IExpressionDecoder<TVar, TExpr> decoder, EnvironmentDomain<TVar, TInterval> varsToInterval )
        {
            this.Decoder = decoder;
            this.varsToIntervals = varsToInterval;
        }

        protected IntervalEnvironmentBase(IExpressionDecoder<TVar, TExpr> decoder)
            : this(decoder, EnvironmentDomain<TVar, TInterval>.TopValue(null))
        { 
        }

        public TInterval Eval (TExpr expr)
        {
            int intValue;
            if (Decoder.IsConstantInt(expr, out intValue))
                return Context.For (intValue);

            var evaluator = new EvaluateExpressionVisitor<TEnv, TVar, TExpr, TInterval, TNumeric>(Decoder);
            var interval = evaluator.Visit (expr, new Counter<TEnv> (this as TEnv));

            if (evaluator.DuplicatedOccurences.Length () >= 1)
            {
                bool noDuplicates = true;
                TInterval result = null;
                foreach (var var in evaluator.DuplicatedOccurences.AsEnumerable ())
                {
                    TInterval intv;
                    if (this.TryGetValue (var, out intv) && intv.IsFinite && this.Context.IsGreaterEqualThanZero (intv.LowerBound))
                    {
                        var extreme = this.EvalWithExtremes (expr, var, intv);
                        if (noDuplicates)
                        {
                            noDuplicates = false;
                            result = extreme;
                        }
                        else
                            result = result.Join (extreme);
                    }
                }

                if (!noDuplicates)
                    interval = result;
            }

            return interval;
        }

        public TInterval Eval (TVar var)
        {
            TInterval intv;
            if (this.TryGetValue(var, out intv))
                return intv;

            return Context.TopValue;
        }

        /// <summary>
        /// Evaluates expression with variable equal to var's lowerbound or upperbound.
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="var"></param>
        /// <param name="intv"></param>
        /// <returns></returns>
        private TInterval EvalWithExtremes(TExpr expr, TVar var, TInterval intv)
        {
            var evaluator = new EvaluateExpressionVisitor<TEnv, TVar, TExpr, TInterval, TNumeric> (Decoder);

            var envLowerBound = this.With (var, Context.For (intv.LowerBound)); // replace current intv with it's only lowerbound
            var withLowerBound = evaluator.Visit (expr, new Counter<TEnv> (envLowerBound));

            var envUpperBound = this.With (var, Context.For (intv.UpperBound)); // replace current intv with it's only upperBound
            var withUpperBound = evaluator.Visit (expr, new Counter<TEnv> (envUpperBound));

            return withLowerBound.Join (withUpperBound);
        }

        public string ToString(TExpr expr)
        {
            if (this.IsBottom)
                return this.BottomSymbolIfAny ();
            if (this.IsTop)
                return "Top";

            var list = new List<string> ();
            foreach (var variable in this.Variables)
            {
                var intv = this.varsToIntervals[variable];
                if (!intv.IsTop)
                {
                    var name = this.Decoder != null ? this.Decoder.NameOf (variable) : variable.ToString ();
                    list.Add (name + ": " + intv);
                }
            }
            list.Sort();
            return string.Join(", ", list);
        }

        public TEnv AssumeTrue(TExpr guard)
        {
            Assumer.AssumeNotEqualToZero (Decoder.UnderlyingVariable (guard), this as TEnv);
            return new IntervalTestVisitor (Decoder).VisitTrue (guard, this as TEnv);
        }

        public TEnv AssumeFalse(TExpr guard)
        {
            Assumer.AssumeEqualToZero (Decoder.UnderlyingVariable (guard), this as TEnv);
            return new IntervalTestVisitor (Decoder).VisitFalse (guard, this as TEnv);
        }

        public FlatDomain<bool> CheckIfHolds (TExpr expr)
        {
            throw new System.NotImplementedException ();
        }

        public bool Contains(TVar v)
        {
            return this.varsToIntervals.Contains (v);
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

            if (interval.IsBottom)
                return Bottom;

            return this.With(var, interval);
        }

        #region Implementation of IAbstractDomain<T>

        public TEnv Top { get { return NewInstance (EnvironmentDomain<TVar, TInterval>.TopValue (null)); } }

        public TEnv Bottom { get { return NewInstance (EnvironmentDomain<TVar, TInterval>.BottomValue()); } }

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
            private readonly IntervalAssumeTrueVisitor<TEnv, TVar, TExpr, TInterval, TNumeric> trueVisitor;
            private readonly IntervalAssumeFalseVisitor<TEnv, TVar, TExpr, TInterval, TNumeric> falseVisitor;

            public IntervalTestVisitor(IExpressionDecoder<TVar, TExpr> decoder)
            {
                this.trueVisitor = new IntervalAssumeTrueVisitor<TEnv, TVar, TExpr, TInterval, TNumeric>(decoder);
                this.falseVisitor = new IntervalAssumeFalseVisitor<TEnv, TVar, TExpr, TInterval, TNumeric>(decoder);

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