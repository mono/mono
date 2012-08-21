// 
// IntervalInference.cs
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

using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        static class IntervalInference {
                public static class ConstraintsFor {
                        public static IDictionary<TVar, Sequence<TInterval>> GreaterEqualThanZero<TEnv, TVar, TExpr, TInterval>
                                (TExpr expr, IExpressionDecoder<TVar, TExpr> decoder, TEnv env)
                                where TEnv : IIntervalEnvironment<TVar, TExpr, TInterval, Rational>
                                where TVar : IEquatable<TVar>
                                where TInterval : IntervalBase<TInterval, Rational>
                        {
                                var result = new Dictionary<TVar, Sequence<TInterval>> ();
                                var variable = decoder.UnderlyingVariable (expr);

                                AddToResult (result, variable, env.Eval (expr).Meet (env.Context.Positive));

                                if (!decoder.IsVariable (expr)) {
                                        Polynomial<TVar, TExpr> zeroPoly; // poly(0)
                                        if (!Polynomial<TVar, TExpr>.TryToPolynomial (new[] {Monomial<TVar>.From (Rational.Zero)}, out zeroPoly))
                                                throw new AbstractInterpretationException (
                                                        "It can never be the case that the conversion of a list of monomials into a polynomial fails.");

                                        Polynomial<TVar, TExpr> exprPoly; // poly(expr)
                                        Polynomial<TVar, TExpr> fullPoly; // '0 <= poly(expr)' polynome
                                        if (Polynomial<TVar, TExpr>.TryBuildFrom (expr, decoder, out exprPoly) &&
                                            Polynomial<TVar, TExpr>.TryToPolynomial (ExpressionOperator.LessEqualThan, zeroPoly, exprPoly, out fullPoly) &&
                                            fullPoly.IsIntervalForm) {
                                                var k = fullPoly.Left[0].Coeff; // k != 0
                                                TVar x;
                                                fullPoly.Left[0].IsSingleVariable (out x);

                                                Rational constraint;
                                                if (Rational.TryDivide (fullPoly.Right[0].Coeff, k, out constraint)) {
                                                        TInterval interval;
                                                        if (k > 0L) // +x <= constraint
                                                                interval = env.Eval (x).Meet (env.Context.For (Rational.MinusInfinity, constraint));
                                                        else // -x <= -constraint ==> x >= constraint
                                                                interval = env.Eval (x).Meet (env.Context.For (constraint, Rational.PlusInfinity));

                                                        AddToResult (result, x, interval);
                                                }
                                        }
                                }
                                return result;
                        }

                        public static IDictionary<TVar, Sequence<TInterval>> LessEqualThan<TEnv, TVar, TExpr, TInterval>
                                (TExpr left, TExpr right, IExpressionDecoder<TVar, TExpr> decoder, TEnv env, out bool isBottom)
                                where TEnv : IIntervalEnvironment<TVar, TExpr, TInterval, Rational>
                                where TVar : IEquatable<TVar>
                                where TInterval : IntervalBase<TInterval, Rational>
                        {
                                isBottom = false;
                                var result = new Dictionary<TVar, Sequence<TInterval>> ();

                                if (IsFloat (left, decoder) || IsFloat (right, decoder))
                                        return result;

                                var leftIntv = env.Eval (left);
                                var rightIntv = env.Eval (right);

                                var leftVar = decoder.UnderlyingVariable (left);
                                var rightVar = decoder.UnderlyingVariable (right);

                                TInterval refinedIntv;
                                if (TryRefineLessEqualThan<TEnv, TVar, TExpr, TInterval> (leftIntv, rightVar, env, out refinedIntv))
                                        AddToResult (result, rightVar, refinedIntv);

                                if (TryRefineLeftLessEqualThanK<TEnv, TVar, TExpr, TInterval> (leftVar, rightIntv, env, out refinedIntv))
                                        AddToResult (result, leftVar, refinedIntv);

                                Polynomial<TVar, TExpr> poly;
                                Polynomial<TVar, TExpr> leftPoly;
                                Polynomial<TVar, TExpr> rightPoly;

                                if (Polynomial<TVar, TExpr>.TryBuildFrom (left, decoder, out leftPoly) &&
                                    Polynomial<TVar, TExpr>.TryBuildFrom (right, decoder, out rightPoly) &&
                                    Polynomial<TVar, TExpr>.TryToPolynomial (ExpressionOperator.LessEqualThan, leftPoly, rightPoly, out poly) &&
                                    poly.IsLinear) {
                                        if (poly.Left.Length == 1)
                                                return TestTrueLessEqualThan_AxLeqK (poly, env, result, out isBottom);
                                        if (poly.Left.Length == 2)
                                                return TestTrueLessEqualThan_AxByLeqK (poly, env, result, out isBottom);
                                }

                                return result;
                        }

                        public static IDictionary<TVar, Sequence<TInterval>> LessThan<TEnv, TVar, TExpr, TInterval>
                                (TExpr left, TExpr right, IExpressionDecoder<TVar, TExpr> decoder, TEnv env, out bool isBottom)
                                where TEnv : IIntervalEnvironment<TVar, TExpr, TInterval, Rational>
                                where TVar : IEquatable<TVar>
                                where TInterval : IntervalBase<TInterval, Rational>
                        {
                                isBottom = false;
                                var result = new Dictionary<TVar, Sequence<TInterval>> ();

                                var leftIntv = env.Eval (left);
                                var rightIntv = env.Eval (right);

                                var rightVar = decoder.UnderlyingVariable (right);
                                var successor = IsFloat (left, decoder) || IsFloat (right, decoder) ? Rational.Zero : Rational.One;

                                TInterval refinedIntv;
                                if (TryRefineKLessThanRight<TEnv, TVar, TExpr, TInterval> (leftIntv, rightVar, successor, env, out refinedIntv) && !refinedIntv.IsSinglePoint)
                                        AddToResult (result, rightVar, refinedIntv);

                                if (successor.IsZero)
                                        return result;

                                var leftVar = decoder.UnderlyingVariable (left);
                                if (TryRefineLessThan<TEnv, TVar, TExpr, TInterval> (leftVar, rightIntv, env, out refinedIntv) && !refinedIntv.IsSinglePoint)
                                        AddToResult (result, leftVar, refinedIntv);

                                Polynomial<TVar, TExpr> poly;
                                Polynomial<TVar, TExpr> leftPoly;
                                Polynomial<TVar, TExpr> rightPoly;

                                if (Polynomial<TVar, TExpr>.TryBuildFrom (left, decoder, out leftPoly) &&
                                    Polynomial<TVar, TExpr>.TryBuildFrom (right, decoder, out rightPoly) &&
                                    Polynomial<TVar, TExpr>.TryToPolynomial (ExpressionOperator.LessThan, leftPoly, rightPoly, out poly) &&
                                    poly.IsLinear) {
                                        if (poly.Left.Length == 1)
                                                return TestTrueLessEqualThan_AxLtK (poly, env, result, out isBottom);
                                        if (poly.Left.Length == 2)
                                                return TestTrueLessEqualThan_AxByLtK (poly, env, result, out isBottom);
                                }

                                return result;
                        }

                        /// <summary>
                        /// Get interval for 'left' in inequation 'left &lt;= k'.
                        /// </summary>
                        public static bool TryRefineLeftLessEqualThanK<TEnv, TVar, TExpr, TInterval> (TVar left, TInterval k, TEnv env, out TInterval refined)
                                where TEnv : IIntervalEnvironment<TVar, TExpr, TInterval, Rational>
                                where TInterval : IntervalBase<TInterval, Rational>
                        {
                                if (!k.IsNormal ())
                                        return false.Without (out refined);

                                var interval = env.Context.For (Rational.MinusInfinity, k.UpperBound);

                                TInterval leftIntv;
                                if (env.TryGetValue (left, out leftIntv))
                                        interval = interval.Meet (leftIntv);

                                return true.With (interval, out refined);
                        }

                        /// <summary>
                        /// Get interval for 'left' in inequation 'left &lt; k'.
                        /// </summary>
                        public static bool TryRefineLessThan<TEnv, TVar, TExpr, TInterval> (TVar left, TInterval k, TEnv env, out TInterval refined)
                                where TEnv : IIntervalEnvironment<TVar, TExpr, TInterval, Rational>
                                where TInterval : IntervalBase<TInterval, Rational>
                        {
                                if (!k.IsNormal ())
                                        return false.Without (out refined);

                                var interval = env.Context.For (Rational.MinusInfinity, k.UpperBound.IsInteger ? k.UpperBound - 1L : k.UpperBound);

                                TInterval leftIntv;
                                if (env.TryGetValue (left, out leftIntv))
                                        interval = interval.Meet (leftIntv);

                                return true.With (interval, out refined);
                        }

                        /// <summary>
                        /// Get interval for 'right' in inequation 'k &lt;= right'.
                        /// </summary>
                        public static bool TryRefineLessEqualThan<TEnv, TVar, TExpr, TInterval> (TInterval k, TVar right, TEnv env, out TInterval refined)
                                where TEnv : IIntervalEnvironment<TVar, TExpr, TInterval, Rational>
                                where TInterval : IntervalBase<TInterval, Rational>
                        {
                                if (!k.IsNormal ())
                                        return false.Without (out refined);

                                var interval = env.Context.RightOpen (k.LowerBound);

                                TInterval rightIntv;
                                if (env.TryGetValue (right, out rightIntv))
                                        interval = interval.Meet (rightIntv);

                                return true.With (interval, out refined);
                        }

                        /// <summary>
                        /// Get interval for 'right' in inequation 'k &lt; right'.
                        /// </summary>
                        public static bool TryRefineKLessThanRight<TEnv, TVar, TExpr, TInterval> (TInterval k, TVar right, Rational successor, TEnv env, out TInterval refined)
                                where TEnv : IIntervalEnvironment<TVar, TExpr, TInterval, Rational>
                                where TInterval : IntervalBase<TInterval, Rational>
                        {
                                if (!k.IsNormal ())
                                        return false.Without (out refined);

                                // [k, +oo] or (k, +oo]
                                var interval = env.Context.For (k.LowerBound.IsInteger ? k.LowerBound + successor : k.LowerBound, Rational.PlusInfinity);

                                TInterval rightIntv;
                                if (env.TryGetValue (right, out rightIntv))
                                        interval = interval.Meet (rightIntv);

                                return true.With (interval, out refined);
                        }

                        public static void NotEqual<TEnv, TVar, TExpr, TInterval>
                                (TExpr left, TExpr right, IExpressionDecoder<TVar, TExpr> decoder, TEnv env, out InferenceResult<TVar, TInterval> resultLeft,
                                 out InferenceResult<TVar, TInterval> resultRight) where TInterval : IntervalBase<TInterval, Rational>
                                where TEnv : IIntervalEnvironment<TVar, TExpr, TInterval, Rational>
                                where TVar : IEquatable<TVar>
                        {
                                resultLeft = InferenceResult<TVar, TInterval>.Empty;
                                resultRight = InferenceResult<TVar, TInterval>.Empty;

                                var leftIntv = env.Eval (left);
                                var rightIntv = env.Eval (right);

                                var leftVar = decoder.UnderlyingVariable (left);
                                var rightVar = decoder.UnderlyingVariable (right);

                                var successor = IsFloat (left, decoder) || IsFloat (right, decoder) ? Rational.Zero : Rational.One;

                                // l != r <==> l < r && r < l
                                LessThanRefinement<TEnv, TVar, TExpr, TInterval> (successor, env, leftIntv, rightIntv, leftVar, rightVar, ref resultLeft);
                                LessThanRefinement<TEnv, TVar, TExpr, TInterval> (successor, env, rightIntv, leftIntv, rightVar, leftVar, ref resultRight);
                        }

                        static void LessThanRefinement<TEnv, TVar, TExpr, TInterval>
                                (Rational successor, TEnv env,
                                 TInterval leftIntv, TInterval rightIntv,
                                 TVar leftVar, TVar rightVar,
                                 ref InferenceResult<TVar, TInterval> result)
                                where TEnv : IIntervalEnvironment<TVar, TExpr, TInterval, Rational>
                                where TInterval : IntervalBase<TInterval, Rational>
                                where TVar : IEquatable<TVar>
                        {
                                TInterval refined;
                                if (TryRefineKLessThanRight<TEnv, TVar, TExpr, TInterval> (leftIntv, rightVar, successor, env, out refined))
                                        result = result.AddConstraintFor (rightVar, refined);
                                if (TryRefineLessThan<TEnv, TVar, TExpr, TInterval> (leftVar, rightIntv, env, out refined))
                                        result = result.AddConstraintFor (leftVar, refined);
                        }

                        static IDictionary<TVar, Sequence<TInterval>> TestTrueLessEqualThan_AxByLeqK<TEnv, TVar, TExpr, TInterval>
                                (Polynomial<TVar, TExpr> poly, TEnv env, IDictionary<TVar, Sequence<TInterval>> result, out bool isBottom)
                                where TEnv : IIntervalEnvironment<TVar, TExpr, TInterval, Rational>
                                where TVar : IEquatable<TVar>
                                where TInterval : IntervalBase<TInterval, Rational>
                        {
                                // ax + by <= k
                                var ax = poly.Left[0];
                                var by = poly.Left[1];

                                TVar x, y;
                                ax.IsSingleVariable (out x);
                                by.IsSingleVariable (out y);

                                var k = poly.Right[0].Coeff;
                                var a = ax.Coeff;
                                var b = by.Coeff;

                                var aInterval = env.Context.For (a);
                                var bInterval = env.Context.For (b);
                                var kInterval = env.Context.For (k);

                                var xInterval = env.Eval (x);
                                var yInterval = env.Eval (y);

                                IntervalContextBase<TInterval, Rational> ctx = env.Context;

                                // x <= (k - (b * y)) / a;
                                var boundingInterval = ctx.Div (ctx.Sub (kInterval, ctx.Mul (bInterval, yInterval)), aInterval);
                                Func<Rational, TInterval> upperBounded = (i) => ctx.For (Rational.MinusInfinity, i);
                                Func<Rational, TInterval> lowerBounded = (i) => ctx.For (i, Rational.PlusInfinity);

                                if (BoundVariable (ctx, a, xInterval, boundingInterval, x, result, out isBottom, upperBounded, lowerBounded))
                                        return result;

                                // y <= (k - (a * x)) / b;
                                boundingInterval = ctx.Div (ctx.Sub (kInterval, ctx.Mul (aInterval, xInterval)), bInterval);
                                if (BoundVariable (ctx, b, yInterval, boundingInterval, y, result, out isBottom, upperBounded, lowerBounded))
                                        return result;

                                return result;
                        }

                        static IDictionary<TVar, Sequence<TInterval>> TestTrueLessEqualThan_AxByLtK<TEnv, TVar, TExpr, TInterval>
                                (Polynomial<TVar, TExpr> poly, TEnv env, IDictionary<TVar, Sequence<TInterval>> result, out bool isBottom)
                                where TEnv : IIntervalEnvironment<TVar, TExpr, TInterval, Rational>
                                where TVar : IEquatable<TVar>
                                where TInterval : IntervalBase<TInterval, Rational>
                        {
                                // ax + by <= k
                                var ax = poly.Left[0];
                                var by = poly.Left[1];

                                TVar x, y;
                                ax.IsSingleVariable (out x);
                                by.IsSingleVariable (out y);

                                var k = poly.Right[0].Coeff;
                                var a = ax.Coeff;
                                var b = by.Coeff;

                                var aInterval = env.Context.For (a);
                                var bInterval = env.Context.For (b);
                                var kInterval = env.Context.For (k);

                                var xInterval = env.Eval (x);
                                var yInterval = env.Eval (y);

                                IntervalContextBase<TInterval, Rational> ctx = env.Context;
                                Func<Rational, TInterval> upperBounded =
                                        (i) => ctx.For (Rational.MinusInfinity, !i.IsInteger ? i.PreviousInt32 : i - 1L);
                                Func<Rational, TInterval> lowerBounded =
                                        (i) => ctx.For (!i.IsInteger ? i.NextInt32 : i + 1L, Rational.PlusInfinity);

                                // x <= (k - (b * y)) / a;
                                var boundingInterval = ctx.Div (ctx.Sub (kInterval, ctx.Mul (bInterval, yInterval)), aInterval);
                                if (BoundVariable (ctx, a, xInterval, boundingInterval, x, result, out isBottom, upperBounded, lowerBounded))
                                        return result;

                                // y <= (k - (a * x)) / b;
                                boundingInterval = ctx.Div (ctx.Sub (kInterval, ctx.Mul (aInterval, xInterval)), bInterval);
                                if (BoundVariable (ctx, b, yInterval, boundingInterval, y, result, out isBottom, upperBounded, lowerBounded))
                                        return result;

                                return result;
                        }

                        /// <summary>
                        /// Get constraints for variables from polynome in form 'a*x &lt;= k'
                        /// </summary>
                        /// <param name="poly">Polynome in canonical form. Two monomes involved. </param>
                        static IDictionary<TVar, Sequence<TInterval>> TestTrueLessEqualThan_AxLeqK<TEnv, TVar, TExpr, TInterval>
                                (Polynomial<TVar, TExpr> poly, TEnv env, IDictionary<TVar, Sequence<TInterval>> result, out bool isBottom)
                                where TEnv : IIntervalEnvironment<TVar, TExpr, TInterval, Rational>
                                where TVar : IEquatable<TVar>
                                where TInterval : IntervalBase<TInterval, Rational>
                        {
                                isBottom = false;

                                var ax = poly.Left[0];
                                var k = poly.Right[1];
                                if (ax.IsConstant) {
                                        if (ax.Coeff > k.Coeff) {
                                                isBottom = true;
                                                return result;
                                        }
                                }
                                else {
                                        TVar x;
                                        ax.IsSingleVariable (out x);
                                        Rational div;
                                        if (Rational.TryDivide (k.Coeff, ax.Coeff, out div)) {
                                                var intv = env.Eval (x);

                                                var refined = ax.Coeff.Sign < 1
                                                                      ? intv.Meet (env.Context.For (div, Rational.PlusInfinity))
                                                                      : intv.Meet (env.Context.For (Rational.MinusInfinity, div));

                                                AddToResult (result, x, refined);
                                        }
                                }

                                return result;
                        }

                        /// <summary>
                        /// Get constraints for variables from polynome in form 'a*x &lt;= k'
                        /// </summary>
                        /// <param name="poly">Polynome in canonical form. Two monomes involved. </param>
                        static IDictionary<TVar, Sequence<TInterval>> TestTrueLessEqualThan_AxLtK<TEnv, TVar, TExpr, TInterval>
                                (Polynomial<TVar, TExpr> poly, TEnv env, IDictionary<TVar, Sequence<TInterval>> result, out bool isBottom)
                                where TEnv : IIntervalEnvironment<TVar, TExpr, TInterval, Rational>
                                where TVar : IEquatable<TVar>
                                where TInterval : IntervalBase<TInterval, Rational>
                        {
                                isBottom = false;

                                var ax = poly.Left[0];
                                var k = poly.Right[1];
                                if (ax.IsConstant) {
                                        if (ax.Coeff >= k.Coeff) {
                                                isBottom = true;
                                                return result;
                                        }
                                }
                                else {
                                        TVar x;
                                        ax.IsSingleVariable (out x);
                                        Rational div;
                                        if (Rational.TryDivide (k.Coeff, ax.Coeff, out div)) {
                                                var intv = env.Eval (x);

                                                var boundByDivPlus = !div.IsInteger
                                                                             ? env.Context.For (div.NextInt32, Rational.PlusInfinity)
                                                                             : env.Context.For (div + 1L, Rational.PlusInfinity);
                                                var boundByDivMinus = !div.IsInteger
                                                                              ? env.Context.For (Rational.MinusInfinity, div.NextInt32 - 1L)
                                                                              : env.Context.For (Rational.MinusInfinity, div - 1L);
                                                var refined = intv.Meet (ax.Coeff.Sign < 1 ? boundByDivPlus : boundByDivMinus);

                                                if (refined.IsBottom) {
                                                        isBottom = true;
                                                        return result;
                                                }

                                                AddToResult (result, x, refined);
                                        }
                                }
                                return result;
                        }

                        static bool BoundVariable<TVar, TInterval>
                                (IntervalContextBase<TInterval, Rational> ctx, Rational a, TInterval xIntervalOld, TInterval boundingInterval, TVar x,
                                 IDictionary<TVar, Sequence<TInterval>> result, out bool isBottom,
                                 Func<Rational, TInterval> upperBounded, Func<Rational, TInterval> lowerBounded)
                                where TVar : IEquatable<TVar>
                                where TInterval : IntervalBase<TInterval, Rational>
                        {
                                isBottom = false;
                                if (a.IsZero) {
                                        TInterval boundingForVariable;
                                        if (a.Sign > 0L)
                                                boundingForVariable = upperBounded (boundingInterval.UpperBound);
                                        else
                                                boundingForVariable = lowerBounded (boundingInterval.LowerBound);

                                        var refined = xIntervalOld.Meet (boundingForVariable);
                                        if (refined.IsBottom) {
                                                isBottom = true;
                                                return true;
                                        }

                                        AddToResult (result, x, refined);
                                }
                                return false;
                        }

                        static bool IsFloat<TVar, TExpr> (TExpr expr, IExpressionDecoder<TVar, TExpr> decoder)
                        {
                                if (decoder == null)
                                        return false;

                                var type = decoder.TypeOf (expr);
                                return type == ExpressionType.Float32 || type == ExpressionType.Float64;
                        }

                        static void AddToResult<TVar, TInterval> (IDictionary<TVar, Sequence<TInterval>> result, TVar variable, TInterval intv)
                                where TVar : IEquatable<TVar>
                                where TInterval : IntervalBase<TInterval, Rational>
                        {
                                Sequence<TInterval> value;
                                result.TryGetValue (variable, out value);

                                result[variable] = value.Cons (intv);
                        }
                }

                public struct InferenceResult<TVar, TInterval>
                        where TInterval : IAbstractDomain<TInterval>
                        where TVar : IEquatable<TVar> {
                        public static readonly InferenceResult<TVar, TInterval> Empty =
                                new InferenceResult<TVar, TInterval> (false, ImmutableMap<TVar, Sequence<TInterval>>.Empty);

                        public readonly bool IsBottom;

                        public readonly IImmutableMap<TVar, Sequence<TInterval>> Constraints;

                        InferenceResult (bool isbottom, IImmutableMap<TVar, Sequence<TInterval>> map)
                        {
                                Constraints = map;
                                IsBottom = isbottom;
                        }

                        public InferenceResult<TVar, TInterval> AddConstraintFor (TVar rightVar, TInterval refined)
                        {
                                Sequence<TInterval> seq;
                                Constraints.TryGetValue (rightVar, out seq);

                                return new InferenceResult<TVar, TInterval> (IsBottom, Constraints.Add (rightVar, seq.Cons (refined)));
                        }

                        public InferenceResult<TVar, TInterval> Join (InferenceResult<TVar, TInterval> that)
                        {
                                if (IsBottom)
                                        return that;
                                if (that.IsBottom)
                                        return this;

                                var result = Empty;
                                foreach (var var in Constraints.Keys) {
                                        var leftContraints = Constraints[var];
                                        if (leftContraints == null)
                                                continue;

                                        // tops are not included
                                        Sequence<TInterval> rightConstraints;
                                        if (that.Constraints.TryGetValue (var, out rightConstraints) && rightConstraints != null) {
                                                var intv = leftContraints.Head.Join (rightConstraints.Head);
                                                if (!intv.IsTop)
                                                        result.AddConstraintFor (var, intv);
                                        }
                                }

                                return result;
                        }
                 }
        }
}