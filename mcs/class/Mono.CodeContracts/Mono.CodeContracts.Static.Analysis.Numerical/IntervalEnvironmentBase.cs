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

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        abstract class IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> : IIntervalEnvironment<TVar, TExpr, TInterval, TNumeric>,
                                                                                   IEnvironmentDomain<IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric>, TVar, TExpr>
                where TInterval : IntervalBase<TInterval, TNumeric>
                where TVar : IEquatable<TVar> {
                public abstract IntervalAssumerBase<TVar, TExpr, TInterval, TNumeric> Assumer { get; }
                public abstract IntervalContextBase<TInterval, TNumeric> Context { get; }

                public readonly IExpressionDecoder<TVar, TExpr> Decoder;
                readonly EnvironmentDomain<TVar, TInterval> vars_to_intervals;

                public IEnumerable<TVar> Variables { get { return vars_to_intervals.Keys; } }

                protected IntervalEnvironmentBase
                        (IExpressionDecoder<TVar, TExpr> decoder,
                         EnvironmentDomain<TVar, TInterval> varsToInterval)
                {
                        Decoder = decoder;
                        vars_to_intervals = varsToInterval;
                }

                protected IntervalEnvironmentBase (IExpressionDecoder<TVar, TExpr> decoder)
                        : this (decoder, EnvironmentDomain<TVar, TInterval>.TopValue (null))
                {
                }

                public TInterval Eval (TExpr expr)
                {
                        int intValue;
                        if (Decoder.IsConstantInt (expr, out intValue))
                                return Context.For (intValue);

                        var evaluator = new EvaluateExpressionVisitor<IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric>, TVar, TExpr, TInterval, TNumeric> (Decoder);
                        var interval = evaluator.Visit (expr, new Counter<IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric>> (this));

                        if (evaluator.DuplicatedOccurences.Length () >= 1) {
                                var noDuplicates = true;
                                TInterval result = null;
                                foreach (var var in evaluator.DuplicatedOccurences.AsEnumerable ()) {
                                        TInterval intv;
                                        if (TryGetValue (var, out intv) && intv.IsFinite &&
                                            Context.IsGreaterEqualThanZero (intv.LowerBound)) {
                                                var extreme = EvalWithExtremes (expr, var, intv);
                                                if (noDuplicates) {
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
                        if (TryGetValue (var, out intv))
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
                TInterval EvalWithExtremes (TExpr expr, TVar var, TInterval intv)
                {
                        var evaluator = new EvaluateExpressionVisitor<IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric>, TVar, TExpr, TInterval, TNumeric> (Decoder);

                        var envLowerBound = With (var, Context.For (intv.LowerBound));
                        // replace current intv with it's only lowerbound
                        var withLowerBound = evaluator.Visit (expr, new Counter<IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric>> (envLowerBound));

                        var envUpperBound = With (var, Context.For (intv.UpperBound));
                        // replace current intv with it's only upperBound
                        var withUpperBound = evaluator.Visit (expr, new Counter<IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric>> (envUpperBound));

                        return withLowerBound.Join (withUpperBound);
                }

                public string ToString (TExpr expr)
                {
                        if (IsBottom)
                                return "_|_";
                        if (IsTop)
                                return "Top";

                        var list = new List<string> ();
                        foreach (var variable in Variables) {
                                var intv = vars_to_intervals[variable];
                                if (!intv.IsTop) {
                                        var name = Decoder != null
                                                           ? Decoder.NameOf (variable)
                                                           : variable.ToString ();
                                        list.Add (name + ": " + intv);
                                }
                        }
                        list.Sort ();
                        return string.Join (", ", list);
                }

                public IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> AssumeTrue (TExpr guard)
                {
                        Assumer.AssumeNotEqualToZero (Decoder.UnderlyingVariable (guard), this);
                        return new IntervalTestVisitor (Decoder).VisitTrue (guard, this);
                }

                public IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> AssumeFalse (TExpr guard)
                {
                        return Assumer.AssumeEqualToZero (Decoder.UnderlyingVariable (guard), this);
                }

                public FlatDomain<bool> CheckIfHolds (TExpr expr)
                {
                        return FlatDomain<bool>.TopValue;
                }

                public bool Contains (TVar v)
                {
                        return vars_to_intervals.Contains (v);
                }

                public bool TryGetValue (TVar v, out TInterval interval)
                {
                        return vars_to_intervals.TryGetValue (v, out interval);
                }

                public IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> With (TVar var, TInterval interval)
                {
                        return NewInstance (vars_to_intervals.With (var, interval));
                }

                public IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> RefineVariable (TVar var, TInterval interval)
                {
                        TInterval current;
                        if (TryGetValue (var, out current))
                                interval = interval.Meet (current);

                        if (interval.IsBottom)
                                return Bottom;

                        return With (var, interval);
                }

                #region Implementation of IAbstractDomain<T>

                public IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> Top { get { return NewInstance (EnvironmentDomain<TVar, TInterval>.TopValue (null)); } }

                public IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> Bottom { get { return NewInstance (EnvironmentDomain<TVar, TInterval>.BottomValue ()); } }

                public bool IsTop { get { return vars_to_intervals.IsTop; } }

                public bool IsBottom { get { return vars_to_intervals.IsBottom; } }

                public IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> Join (IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> that)
                {
                        return NewInstance (vars_to_intervals.Join (that.vars_to_intervals));
                }

                public IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> Join (IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> that, bool widen, out bool weaker)
                {
                        return NewInstance (vars_to_intervals.Join (that.vars_to_intervals, widen, out weaker));
                }

                public IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> Widen (IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> that)
                {
                        return NewInstance (vars_to_intervals.Widen (that.vars_to_intervals));
                }

                public IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> Meet (IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> that)
                {
                        return NewInstance (vars_to_intervals.Meet (that.vars_to_intervals));
                }

                public bool LessEqual (IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> that)
                {
                        return vars_to_intervals.LessEqual (that.vars_to_intervals);
                }

                public IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> ImmutableVersion ()
                {
                        return this;
                }

                public IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> Clone ()
                {
                        return this;
                }

                public void Dump (TextWriter tw)
                {
                        vars_to_intervals.Dump (tw);
                }

                #endregion

                protected abstract IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> NewInstance (EnvironmentDomain<TVar, TInterval> varsToIntervals);

                class IntervalTestVisitor {
                        readonly IntervalAssumeTrueVisitor<TVar, TExpr, TInterval, TNumeric> true_visitor;
                        readonly IntervalAssumeFalseVisitor<TVar, TExpr, TInterval, TNumeric> false_visitor;

                        public IntervalTestVisitor (IExpressionDecoder<TVar, TExpr> decoder)
                        {
                                true_visitor =
                                        new IntervalAssumeTrueVisitor<TVar, TExpr, TInterval, TNumeric> (decoder);
                                false_visitor =
                                        new IntervalAssumeFalseVisitor<TVar, TExpr, TInterval, TNumeric> (decoder);

                                true_visitor.FalseVisitor = false_visitor;
                                false_visitor.TrueVisitor = true_visitor;
                        }

                        public IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> VisitTrue (TExpr guard, IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> data)
                        {
                                return true_visitor.Visit (guard, data);
                        }

                        public IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> VisitFalse (TExpr guard, IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> data)
                        {
                                return false_visitor.Visit (guard, data);
                        }
                }

                public abstract IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> AssumeVariableIn (TVar var, Interval interval);

                public IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> AssumeLessEqualThan (TExpr left, TExpr right)
                {
                        return Assumer.AssumeLessEqualThan (left, right, this);
                }

                INumericalEnvironmentDomain<TVar, TExpr> IAbstractDomain<INumericalEnvironmentDomain<TVar, TExpr>>.Top { get { return Top; } }

                INumericalEnvironmentDomain<TVar, TExpr> IAbstractDomain<INumericalEnvironmentDomain<TVar, TExpr>>.Bottom { get { return Bottom; } }

                INumericalEnvironmentDomain<TVar, TExpr> IAbstractDomain<INumericalEnvironmentDomain<TVar, TExpr>>.Join (INumericalEnvironmentDomain<TVar, TExpr> that)
                {
                        return Join (that as IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric>);
                }

                INumericalEnvironmentDomain<TVar, TExpr> IAbstractDomain<INumericalEnvironmentDomain<TVar, TExpr>>.Join
                        (INumericalEnvironmentDomain<TVar, TExpr> that, bool widen, out bool weaker)
                {
                        return Join (that as IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric>, widen,
                                     out weaker);
                }

                INumericalEnvironmentDomain<TVar, TExpr> IAbstractDomain<INumericalEnvironmentDomain<TVar, TExpr>>.Widen (INumericalEnvironmentDomain<TVar, TExpr> that)
                {
                        return Widen (that as IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric>);
                }

                INumericalEnvironmentDomain<TVar, TExpr> IAbstractDomain<INumericalEnvironmentDomain<TVar, TExpr>>.Meet (INumericalEnvironmentDomain<TVar, TExpr> that)
                {
                        return Meet (that as IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric>);
                }

                bool IAbstractDomain<INumericalEnvironmentDomain<TVar, TExpr>>.LessEqual (INumericalEnvironmentDomain<TVar, TExpr> that)
                {
                        return LessEqual (that as IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric>);
                }

                INumericalEnvironmentDomain<TVar, TExpr> IAbstractDomain<INumericalEnvironmentDomain<TVar, TExpr>>.ImmutableVersion ()
                {
                        return ImmutableVersion ();
                }

                INumericalEnvironmentDomain<TVar, TExpr> IAbstractDomain<INumericalEnvironmentDomain<TVar, TExpr>>.Clone ()
                {
                        return Clone ();
                }

                INumericalEnvironmentDomain<TVar, TExpr> IEnvironmentDomain<INumericalEnvironmentDomain<TVar, TExpr>, TVar, TExpr>.AssumeTrue (TExpr guard)
                {
                        return AssumeTrue (guard);
                }

                INumericalEnvironmentDomain<TVar, TExpr> IEnvironmentDomain<INumericalEnvironmentDomain<TVar, TExpr>, TVar, TExpr>.AssumeFalse (TExpr guard)
                {
                        return AssumeFalse (guard);
                }

                INumericalEnvironmentDomain<TVar, TExpr> INumericalEnvironmentDomain<TVar, TExpr>.AssumeVariableIn (TVar var, Interval interval)
                {
                        return AssumeVariableIn (var, interval);
                }

                INumericalEnvironmentDomain<TVar, TExpr> INumericalEnvironmentDomain<TVar, TExpr>.AssumeLessEqualThan (TExpr left, TExpr right)
                {
                        return AssumeLessEqualThan (left, right);
                }
                }
}