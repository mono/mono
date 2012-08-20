// 
// EvaluateExpressionVisitor.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
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

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class EvaluateExpressionVisitor<TEnv, TVar, TExpr, TInterval, TNumeric> :
                GenericExpressionVisitor<Counter<TEnv>, TInterval, TVar, TExpr>
                where TEnv : IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric>
                where TVar : IEquatable<TVar>
                where TInterval : IntervalBase<TInterval, TNumeric> {
                readonly
                        ConstToIntervalEvaluator
                                <IntervalContextBase<TInterval, TNumeric>, TVar, TExpr, TInterval, TNumeric> constToIntv;

                readonly VariableOccurences occurences;

                public Sequence<TVar> DuplicatedOccurences { get { return occurences.Duplicated; } }

                public EvaluateExpressionVisitor (IExpressionDecoder<TVar, TExpr> decoder)
                        : base (decoder)
                {
                        occurences = new VariableOccurences (decoder);
                        constToIntv =
                                new ConstToIntervalEvaluator
                                        <IntervalContextBase<TInterval, TNumeric>, TVar, TExpr, TInterval, TNumeric> (
                                        decoder);
                }

                public override TInterval Visit (TExpr expr, Counter<TEnv> data)
                {
                        if (data.Count >= 10) // to avoid recursion if any
                                return Default (data);

                        var intv = base.Visit (expr, data.Incremented ());
                        if (intv == null)
                                return Default (data);

                        intv = RefineWithTypeRanges (intv, expr, data.Env);

                        var var = Decoder.UnderlyingVariable (expr);

                        TInterval current;
                        if (data.Env.TryGetValue (var, out current))
                                intv = intv.Meet (current);

                        return intv;
                }

                public override TInterval VisitConstant (TExpr expr, Counter<TEnv> data)
                {
                        return constToIntv.Visit (expr, data.Env.Context);
                }

                public override TInterval VisitAddition (TExpr left, TExpr right, TExpr original, Counter<TEnv> data)
                {
                        return DefaultBinary (left, right, data, (d, l, r) => d.Env.Context.Add (l, r));
                }

                public override TInterval VisitDivision (TExpr left, TExpr right, TExpr original, Counter<TEnv> data)
                {
                        return DefaultBinary (left, right, data, (d, l, r) => d.Env.Context.Div (l, r));
                }

                public override TInterval VisitMultiply (TExpr left, TExpr right, TExpr original, Counter<TEnv> data)
                {
                        return DefaultBinary (left, right, data, (d, l, r) => d.Env.Context.Mul (l, r));
                }

                public override TInterval VisitSubtraction (TExpr left, TExpr right, TExpr original, Counter<TEnv> data)
                {
                        return DefaultBinary (left, right, data, (d, l, r) => d.Env.Context.Sub (l, r));
                }

                public override TInterval VisitEqual (TExpr left, TExpr right, TExpr original, Counter<TEnv> data)
                {
                        return DefaultComparisons (left, right, data);
                }

                public override TInterval VisitLessThan (TExpr left, TExpr right, TExpr original, Counter<TEnv> data)
                {
                        return DefaultComparisons (left, right, data);
                }

                public override TInterval VisitGreaterEqualThan (TExpr left, TExpr right, TExpr original,
                                                                 Counter<TEnv> data)
                {
                        return DefaultComparisons (left, right, data);
                }

                public override TInterval VisitLessEqualThan (TExpr left, TExpr right, TExpr original,
                                                              Counter<TEnv> data)
                {
                        return DefaultComparisons (left, right, data);
                }

                public override TInterval VisitGreaterThan (TExpr left, TExpr right, TExpr original, Counter<TEnv> data)
                {
                        return DefaultComparisons (left, right, data);
                }

                public override TInterval VisitUnknown (TExpr expr, Counter<TEnv> data)
                {
                        occurences.Add (expr);

                        return Default (data);
                }

                protected override TInterval Default (Counter<TEnv> data)
                {
                        return data.Env.Context.TopValue;
                }

                delegate TInterval BinaryEvaluator (Counter<TEnv> env, TInterval left, TInterval right);

                TInterval DefaultBinary (TExpr left, TExpr right, Counter<TEnv> data, BinaryEvaluator binop)
                {
                        occurences.Add (left, right);

                        var incremented = data.Incremented ();
                        var leftIntv = Visit (left, incremented);
                        var rightIntv = Visit (right, incremented);

                        return binop (data, leftIntv, rightIntv);
                }

                TInterval DefaultComparisons (TExpr left, TExpr right, Counter<TEnv> data)
                {
                        occurences.Add (left, right);

                        return Default (data);
                }

                TInterval RefineWithTypeRanges (TInterval intv, TExpr expr, TEnv env)
                {
                        switch (Decoder.TypeOf (expr)) {
                        case ExpressionType.Int32:
                                return env.Context.ApplyConversion (ExpressionOperator.ConvertToInt32, intv);
                        case ExpressionType.Bool:
                                return env.Context.ApplyConversion (ExpressionOperator.ConvertToInt32, intv);
                        default:
                                return intv;
                        }
                }

                class VariableOccurences {
                        readonly Dictionary<TVar, int> occurences;

                        readonly IExpressionDecoder<TVar, TExpr> decoder;

                        Sequence<TVar> duplicated;

                        public VariableOccurences (IExpressionDecoder<TVar, TExpr> decoder)
                        {
                                this.decoder = decoder;
                                occurences = new Dictionary<TVar, int> ();
                                duplicated = Sequence<TVar>.Empty;
                        }

                        public Sequence<TVar> Duplicated { get { return duplicated; } }

                        void Add (TVar var)
                        {
                                int cnt;
                                if (!occurences.TryGetValue (var, out cnt))
                                        cnt = 0;

                                occurences[var] = cnt + 1;

                                if (cnt == 1) // if already was occurence
                                        duplicated = duplicated.Cons (var);
                        }

                        public void Add (TExpr e)
                        {
                                Add (decoder.UnderlyingVariable (e));
                        }

                        public void Add (params TExpr[] exprs)
                        {
                                foreach (var expr in exprs)
                                        Add (expr);
                        }

                        public override string ToString ()
                        {
                                return occurences.ToString ();
                        }
                }
                }
}