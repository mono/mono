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

using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
    static class IntervalInference {
         public static class ConstraintsFor {
             public static EnvironmentDomain<TVar, TInterval> GreaterEqualThanZero<TVar, TExpr, TInterval>(TExpr expr, IExpressionDecoder<TVar, TExpr> decoder, IIntervalEnvironment<TVar, TExpr, TInterval, Rational> env)
                 where TVar : IEquatable<TVar> 
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 var result = EnvironmentDomain<TVar,TInterval>.TopValue();
                 var variable = decoder.UnderlyingVariable (expr);

                 result = result.With (variable, env.Evaluate (expr).Meet (env.Context.Positive));

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

                             result = result.With(x, interval);
                         }
                     }
                 }
                 return result;
             }

             public static EnvironmentDomain<TVar, TInterval> LessEqual<TVar, TExpr, TInterval>(TExpr left, TExpr right, IExpressionDecoder<TVar, TExpr> decoder, IIntervalEnvironment<TVar, TExpr, TInterval, Rational> env)
                 where TVar : IEquatable<TVar>
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 var result = EnvironmentDomain<TVar, TInterval>.TopValue ();

                 if (IsFloat(left, decoder) || IsFloat(right, decoder))
                     return result;

                 var leftIntv = env.Evaluate (left);
                 var rightIntv = env.Evaluate (right);

                 var leftVar  = decoder.UnderlyingVariable (left);
                 var rightVar = decoder.UnderlyingVariable (right);

                 TInterval refinedIntv;
                 if (TryRefineKLessEqualThanRight(leftIntv, rightVar, env, out refinedIntv))
                     result = result.RefineWith (rightVar, refinedIntv);

                 if (TryRefineLeftLessEqualThanK(leftVar, rightIntv, env, out refinedIntv))
                     result = result.RefineWith(leftVar, refinedIntv);

                 Polynomial<TVar, TExpr> poly;
                 Polynomial<TVar, TExpr> leftPoly;
                 Polynomial<TVar, TExpr> rightPoly;

                 if (Polynomial<TVar, TExpr>.TryBuildFrom (left, decoder, out leftPoly) &&
                     Polynomial<TVar, TExpr>.TryBuildFrom (right, decoder, out rightPoly) && 
                     Polynomial<TVar, TExpr>.TryToPolynomial (ExpressionOperator.LessEqualThan, leftPoly, rightPoly, out poly) &&
                     poly.IsLinear)
                 {
                     if (poly.Left.Length == 1)
                         return TestTrueLessEqualThan_AxLeqK (poly, env, result);
                     if (poly.Left.Length == 2)
                         return TestTrueLessEqualThan_AxByLtK(poly, env, result);
                 }

                 return result;
             }

             private static bool TryRefineLeftLessEqualThanK<TVar, TExpr, TInterval>(TVar leftVar, TInterval k, IIntervalEnvironment<TVar, TExpr, TInterval, Rational> env, out TInterval refined) 
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 if (!k.IsNormal())
                     return false.Without(out refined);

                 var interval = env.Context.For(Rational.MinusInfinity, k.LowerBound);

                 TInterval rightIntv;
                 if (env.TryGetValue(leftVar, out rightIntv))
                     interval = interval.Meet(rightIntv);

                 return true.With(interval, out refined);
             }

             private static bool TryRefineKLessEqualThanRight<TVar, TExpr, TInterval>(TInterval k, TVar rightVar, IIntervalEnvironment<TVar, TExpr, TInterval, Rational> env, out TInterval refined) 
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 if (!k.IsNormal())
                     return false.Without (out refined);

                 var interval = env.Context.For (k.UpperBound, Rational.PlusInfinity);

                 TInterval rightIntv;
                 if (env.TryGetValue(rightVar, out rightIntv))
                     interval = interval.Meet (rightIntv);
                 
                 return true.With (interval, out refined);
             }

             private static EnvironmentDomain<TVar, TInterval> TestTrueLessEqualThan_AxByLtK<TVar, TExpr, TInterval>(Polynomial<TVar, TExpr> poly, IIntervalEnvironment<TVar, TExpr, TInterval, Rational> env, EnvironmentDomain<TVar, TInterval> result)
                 where TVar : IEquatable<TVar>
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 throw new NotImplementedException ();
             }

             /// <summary>
             /// Get constraints for variables from polynome in form 'a*x &lt;= k'
             /// </summary>
             /// <param name="poly">Polynome in canonical form. Two monomes involved. </param>
             private static EnvironmentDomain<TVar, TInterval> TestTrueLessEqualThan_AxLeqK<TVar,TExpr,TInterval>(Polynomial<TVar, TExpr> poly, IIntervalEnvironment<TVar, TExpr, TInterval, Rational> env, EnvironmentDomain<TVar, TInterval> result) 
                 where TVar : IEquatable<TVar> 
                 where TInterval : IntervalBase<TInterval, Rational>
             {
                 var ax = poly.Left[0];
                 var k = poly.Right[1];
                 if (ax.IsConstant)
                     if (ax.Coeff > k.Coeff)
                         return result.Bottom;

                 TVar x;
                 ax.IsSingleVariable (out x);
                 Rational div;
                 if (Rational.TryDiv (k.Coeff, ax.Coeff, out div))
                 {
                     var intv = env.Evaluate (x);

                     var refined = ax.Coeff.Sign < 1
                         ? intv.Meet (env.Context.For (div, Rational.PlusInfinity))
                         : intv.Meet (env.Context.For (Rational.MinusInfinity, div));

                     return result.RefineWith(x, refined);
                 }

                 return result;
             }

             private static bool IsFloat<TVar, TExpr> (TExpr expr, IExpressionDecoder<TVar, TExpr> decoder)
             {
                 if (decoder == null)
                     return false;

                 var type = decoder.TypeOf (expr);
                 return type == ExpressionType.Float32 || type == ExpressionType.Float64;
             }
         }
    }
}