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
             public static IDictionary<TVar, Sequence<TInterval>> GreaterEqualThanZero<TVar, TExpr, TInterval>(TExpr expr, IExpressionDecoder<TVar, TExpr> decoder, IIntervalEnvironment<TVar, TExpr, TInterval, Rational> env)
                 where TVar : IEquatable<TVar> 
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 var result = new Dictionary<TVar, Sequence<TInterval>> ();
                 var variable = decoder.UnderlyingVariable (expr);

                 AddToResult (result, variable, env.Evaluate (expr).Meet (env.Context.Positive));

                 if (!decoder.IsVariable (expr))
                 {
                     Polynomial<TVar, TExpr> zeroPoly; // poly(0)
                     if (!Polynomial<TVar, TExpr>.TryToPolynomial (new[] { Monomial<TVar>.From (Rational.Zero) }, out zeroPoly)) 
                         throw new AbstractInterpretationException ("It can never be the case that the conversion of a list of monomials into a polynomial fails.");

                     Polynomial<TVar, TExpr> exprPoly; // poly(expr)
                     Polynomial<TVar, TExpr> fullPoly; // '0 <= poly(expr)' polynome
                     if (Polynomial<TVar, TExpr>.TryBuildFrom (expr, decoder, out exprPoly) &&  
                         Polynomial<TVar, TExpr>.TryToPolynomial (ExpressionOperator.LessEqualThan, zeroPoly, exprPoly, out fullPoly) &&
                         fullPoly.IsIntervalForm)
                     {
                         var k = fullPoly.Left[0].Coeff; // k != 0
                         TVar x;
                         fullPoly.Left[0].IsSingleVariable (out x);
                         
                         Rational constraint;
                         if (Rational.TryDiv (fullPoly.Right[0].Coeff, k, out constraint))
                         {
                             TInterval interval;
                             if (k > 0L) // +x <= constraint
                                 interval = env.Evaluate (x).Meet (env.Context.For (Rational.MinusInfinity, constraint));
                             else        // -x <= -constraint ==> x >= constraint
                                 interval = env.Evaluate (x).Meet (env.Context.For (constraint, Rational.PlusInfinity));

                             AddToResult(result, x, interval);
                         }
                     }
                 }
                 return result;
             }

             public static IDictionary<TVar, Sequence<TInterval>> LessEqual<TVar, TExpr, TInterval>(TExpr left, TExpr right, IExpressionDecoder<TVar, TExpr> decoder, IIntervalEnvironment<TVar, TExpr, TInterval, Rational> env, out bool isBottom)
                 where TVar : IEquatable<TVar>
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 isBottom = false;
                 var result = new Dictionary<TVar, Sequence<TInterval>> ();

                 if (IsFloat(left, decoder) || IsFloat(right, decoder))
                     return result;

                 var leftIntv = env.Evaluate (left);
                 var rightIntv = env.Evaluate (right);

                 var leftVar  = decoder.UnderlyingVariable (left);
                 var rightVar = decoder.UnderlyingVariable (right);

                 TInterval refinedIntv;
                 if (TryRefineKLessEqualThanRight (leftIntv, rightVar, env, out refinedIntv))
                     AddToResult (result, rightVar, refinedIntv);

                 if (TryRefineLeftLessEqualThanK (leftVar, rightIntv, env, out refinedIntv))
                     AddToResult (result, leftVar, refinedIntv);

                 Polynomial<TVar, TExpr> poly;
                 Polynomial<TVar, TExpr> leftPoly;
                 Polynomial<TVar, TExpr> rightPoly;

                 if (Polynomial<TVar, TExpr>.TryBuildFrom (left, decoder, out leftPoly) &&
                     Polynomial<TVar, TExpr>.TryBuildFrom (right, decoder, out rightPoly) && 
                     Polynomial<TVar, TExpr>.TryToPolynomial (ExpressionOperator.LessEqualThan, leftPoly, rightPoly, out poly) &&
                     poly.IsLinear)
                 {
                     if (poly.Left.Length == 1)
                         return TestTrueLessEqualThan_AxLeqK (poly, env, result, out isBottom);
                     if (poly.Left.Length == 2)
                         return TestTrueLessEqualThan_AxByLeqK(poly, env, result, out isBottom);
                 }

                 return result;
             }

             public static IDictionary<TVar, Sequence<TInterval>> LessThan<TVar, TExpr, TInterval>(TExpr left, TExpr right, IExpressionDecoder<TVar, TExpr> decoder, IIntervalEnvironment<TVar, TExpr, TInterval, Rational> env, out bool isBottom) 
                 where TVar : IEquatable<TVar> 
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 isBottom = false;
                 var result = new Dictionary<TVar, Sequence<TInterval>>();

                 var leftIntv = env.Evaluate(left);
                 var rightIntv = env.Evaluate(right);

                 var rightVar = decoder.UnderlyingVariable(right);
                 var successor = IsFloat (left, decoder) || IsFloat (right, decoder) ? Rational.Zero : Rational.One;
                 
                 TInterval refinedIntv;
                 if (TryRefineKLessThanRight(leftIntv, rightVar, successor, env, out refinedIntv) && !refinedIntv.IsSinglePoint)
                     AddToResult(result, rightVar, refinedIntv);

                 if (successor.IsZero)
                     return result;

                 var leftVar = decoder.UnderlyingVariable(left);
                 if (TryRefineLeftLessThanK(leftVar, rightIntv, env, out refinedIntv) && !refinedIntv.IsSinglePoint)
                     AddToResult(result, leftVar, refinedIntv);

                 Polynomial<TVar, TExpr> poly;
                 Polynomial<TVar, TExpr> leftPoly;
                 Polynomial<TVar, TExpr> rightPoly;

                 if (Polynomial<TVar, TExpr>.TryBuildFrom(left, decoder, out leftPoly) &&
                     Polynomial<TVar, TExpr>.TryBuildFrom(right, decoder, out rightPoly) &&
                     Polynomial<TVar, TExpr>.TryToPolynomial(ExpressionOperator.LessThan, leftPoly, rightPoly, out poly) &&
                     poly.IsLinear)
                 {
                     if (poly.Left.Length == 1)
                         return TestTrueLessEqualThan_AxLtK(poly, env, result, out isBottom);
                     if (poly.Left.Length == 2)
                         return TestTrueLessEqualThan_AxByLtK(poly, env, result, out isBottom);
                 }

                 return result;
             }

             /// <summary>
             /// Get interval for 'left' in inequation 'left &lt;= k'.
             /// </summary>
             public static bool TryRefineLeftLessEqualThanK<TVar, TExpr, TInterval>(TVar left, TInterval k, IIntervalEnvironment<TVar, TExpr, TInterval, Rational> env, out TInterval refined) 
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 if (!k.IsNormal())
                     return false.Without(out refined);

                 var interval = env.Context.For(Rational.MinusInfinity, k.UpperBound);

                 TInterval leftIntv;
                 if (env.TryGetValue(left, out leftIntv))
                     interval = interval.Meet(leftIntv);

                 return true.With(interval, out refined);
             }

             /// <summary>
             /// Get interval for 'left' in inequation 'left &lt; k'.
             /// </summary>
             public static bool TryRefineLeftLessThanK<TVar, TExpr, TInterval>(TVar left, TInterval k, IIntervalEnvironment<TVar, TExpr, TInterval, Rational> env, out TInterval refined)
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 if (!k.IsNormal())
                     return false.Without(out refined);

                 var interval = env.Context.For(Rational.MinusInfinity, k.UpperBound.IsInteger ? k.UpperBound - 1L : k.UpperBound);

                 TInterval leftIntv;
                 if (env.TryGetValue(left, out leftIntv))
                     interval = interval.Meet(leftIntv);

                 return true.With(interval, out refined);
             }

             /// <summary>
             /// Get interval for 'right' in inequation 'k &lt;= right'.
             /// </summary>
             private static bool TryRefineKLessEqualThanRight<TVar, TExpr, TInterval>(TInterval k, TVar right, IIntervalEnvironment<TVar, TExpr, TInterval, Rational> env, out TInterval refined) 
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 if (!k.IsNormal())
                     return false.Without (out refined);

                 var interval = env.Context.For (k.LowerBound, Rational.PlusInfinity);

                 TInterval rightIntv;
                 if (env.TryGetValue(right, out rightIntv))
                     interval = interval.Meet (rightIntv);
                 
                 return true.With (interval, out refined);
             }

             /// <summary>
             /// Get interval for 'right' in inequation 'k &lt; right'.
             /// </summary>
             private static bool TryRefineKLessThanRight<TVar, TExpr, TInterval>(TInterval k, TVar right, Rational successor, IIntervalEnvironment<TVar, TExpr, TInterval, Rational> env, out TInterval refined)
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 if (!k.IsNormal())
                     return false.Without(out refined);

                 // [k, +oo] or (k, +oo]
                 var interval = env.Context.For(k.LowerBound.IsInteger ? k.LowerBound + successor : k.LowerBound, Rational.PlusInfinity);

                 TInterval rightIntv;
                 if (env.TryGetValue(right, out rightIntv))
                     interval = interval.Meet(rightIntv);

                 return true.With(interval, out refined);
             }

             private static IDictionary<TVar, Sequence<TInterval>> TestTrueLessEqualThan_AxByLeqK<TVar, TExpr, TInterval>(Polynomial<TVar, TExpr> poly, IIntervalEnvironment<TVar, TExpr, TInterval, Rational> env, IDictionary<TVar, Sequence<TInterval>> result, out bool isBottom)
                 where TVar : IEquatable<TVar>
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 // ax + by <= k
                 var ax = poly.Left[0];
                 var by = poly.Left[1];

                 TVar x, y;
                 ax.IsSingleVariable(out x);
                 by.IsSingleVariable(out y);

                 Rational k = poly.Right[0].Coeff;
                 Rational a = ax.Coeff;
                 Rational b = by.Coeff;

                 var aInterval = env.Context.For(a);
                 var bInterval = env.Context.For(b);
                 var kInterval = env.Context.For(k);

                 var xInterval = env.Evaluate(x);
                 var yInterval = env.Evaluate(y);

                 var ctx = env.Context;

                 // x <= (k - (b * y)) / a;
                 TInterval boundingInterval = ctx.Div(ctx.Sub(kInterval, ctx.Mul(bInterval, yInterval)), aInterval);
                 Func<Rational, TInterval> upperBounded = (i) => ctx.For (Rational.MinusInfinity, i);
                 Func<Rational, TInterval> lowerBounded = (i) => ctx.For (i, Rational.PlusInfinity);

                 if (BoundVariable(ctx, a, xInterval, boundingInterval, x, result, out isBottom, upperBounded, lowerBounded))
                     return result;

                 // y <= (k - (a * x)) / b;
                 boundingInterval = ctx.Div(ctx.Sub(kInterval, ctx.Mul(aInterval, xInterval)), bInterval);
                 if (BoundVariable(ctx, b, yInterval, boundingInterval, y, result, out isBottom, upperBounded, lowerBounded))
                     return result;

                 return result;
             }

             private static IDictionary<TVar, Sequence<TInterval>> TestTrueLessEqualThan_AxByLtK<TVar, TExpr, TInterval>(Polynomial<TVar, TExpr> poly, IIntervalEnvironment<TVar, TExpr, TInterval, Rational> env, IDictionary<TVar, Sequence<TInterval>> result, out bool isBottom)
                 where TVar : IEquatable<TVar>
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 // ax + by <= k
                 var ax = poly.Left[0];
                 var by = poly.Left[1];

                 TVar x, y;
                 ax.IsSingleVariable (out x);
                 by.IsSingleVariable (out y);

                 Rational k = poly.Right[0].Coeff;
                 Rational a = ax.Coeff;
                 Rational b = by.Coeff;

                 var aInterval = env.Context.For (a);
                 var bInterval = env.Context.For (b);
                 var kInterval = env.Context.For (k);

                 var xInterval = env.Evaluate (x);
                 var yInterval = env.Evaluate (y);

                 var ctx = env.Context;
                 Func<Rational, TInterval> upperBounded =
                     (i) => ctx.For (Rational.MinusInfinity, !i.IsInteger ? i.PreviousInt32 : i - 1L);
                 Func<Rational, TInterval> lowerBounded =
                     (i) => ctx.For (!i.IsInteger ? i.NextInt32 : i + 1L, Rational.PlusInfinity);

                 // x <= (k - (b * y)) / a;
                 TInterval boundingInterval = ctx.Div (ctx.Sub (kInterval, ctx.Mul (bInterval, yInterval)), aInterval);
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
             private static IDictionary<TVar, Sequence<TInterval>> TestTrueLessEqualThan_AxLeqK<TVar, TExpr, TInterval>(Polynomial<TVar, TExpr> poly, IIntervalEnvironment<TVar, TExpr, TInterval, Rational> env, IDictionary<TVar, Sequence<TInterval>> result, out bool isBottom) 
                 where TVar : IEquatable<TVar> 
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 isBottom = false;

                 var ax = poly.Left[0];
                 var k = poly.Right[1];
                 if (ax.IsConstant)
                 {
                     if (ax.Coeff > k.Coeff)
                     {
                         isBottom = true;
                         return result;
                     }
                 }
                 else
                 {

                     TVar x;
                     ax.IsSingleVariable (out x);
                     Rational div;
                     if (Rational.TryDiv (k.Coeff, ax.Coeff, out div))
                     {
                         var intv = env.Evaluate (x);

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
             private static IDictionary<TVar, Sequence<TInterval>> TestTrueLessEqualThan_AxLtK<TVar, TExpr, TInterval>(Polynomial<TVar, TExpr> poly, IIntervalEnvironment<TVar, TExpr, TInterval, Rational> env, IDictionary<TVar, Sequence<TInterval>> result, out bool isBottom)
                 where TVar : IEquatable<TVar>
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 isBottom = false;

                 var ax = poly.Left[0];
                 var k = poly.Right[1];
                 if (ax.IsConstant)
                 {
                     if (ax.Coeff >= k.Coeff)
                     {
                         isBottom = true;
                         return result;
                     }
                 }
                 else
                 {

                     TVar x;
                     ax.IsSingleVariable (out x);
                     Rational div;
                     if (Rational.TryDiv (k.Coeff, ax.Coeff, out div))
                     {
                         var intv = env.Evaluate (x);

                         TInterval boundByDivPlus = !div.IsInteger
                                                        ? env.Context.For (div.NextInt32, Rational.PlusInfinity)
                                                        : env.Context.For (div + 1L, Rational.PlusInfinity);
                         TInterval boundByDivMinus = !div.IsInteger
                                                         ? env.Context.For (Rational.MinusInfinity, div.NextInt32 - 1L)
                                                         : env.Context.For (Rational.MinusInfinity, div - 1L);
                         var refined = intv.Meet(ax.Coeff.Sign < 1 ? boundByDivPlus : boundByDivMinus);

                         if (refined.IsBottom)
                         {
                             isBottom = true;
                             return result;
                         }

                         AddToResult (result, x, refined);
                     }
                 }
                 return result;
             }

             private static bool BoundVariable<TVar, TInterval> (IntervalContextBase<TInterval, Rational> ctx, Rational a, TInterval xIntervalOld, TInterval boundingInterval, TVar x, IDictionary<TVar, Sequence<TInterval>> result, out bool isBottom, 
                 Func<Rational, TInterval> upperBounded, Func<Rational, TInterval> lowerBounded) 
                 where TVar : IEquatable<TVar> 
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 isBottom = false;
                 if (a.IsZero)
                 {
                     TInterval boundingForVariable;
                     if (a.Sign > 0L)
                         boundingForVariable = upperBounded (boundingInterval.UpperBound);
                     else
                         boundingForVariable = lowerBounded (boundingInterval.LowerBound);

                     TInterval refined = xIntervalOld.Meet (boundingForVariable);
                     if (refined.IsBottom)
                     {
                         isBottom = true;
                         return true;
                     }

                     AddToResult (result, x, refined);
                 }
                 return false;
             }

             private static bool IsFloat<TVar, TExpr> (TExpr expr, IExpressionDecoder<TVar, TExpr> decoder)
             {
                 if (decoder == null)
                     return false;

                 var type = decoder.TypeOf (expr);
                 return type == ExpressionType.Float32 || type == ExpressionType.Float64;
             }

             private static void AddToResult<TVar, TInterval>(IDictionary<TVar, Sequence<TInterval>> result, TVar variable, TInterval intv)
                 where TVar : IEquatable<TVar>
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 Sequence<TInterval> value;
                 result.TryGetValue (variable, out value);

                 result[variable] = value.Cons (intv);
             }
         }
    }
}