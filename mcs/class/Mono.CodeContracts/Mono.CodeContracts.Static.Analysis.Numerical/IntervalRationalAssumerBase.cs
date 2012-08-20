// 
// IntervalRationalAssumerBase.cs
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

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        abstract class IntervalRationalAssumerBase<TVar, TExpr, TInterval> :
                IntervalAssumerBase<TVar, TExpr, TInterval, Rational>
                where TInterval : IntervalBase<TInterval, Rational>
                where TVar : IEquatable<TVar> {
                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> AssumeLessThan
                        (TExpr left, TExpr right, IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> env)
                {
                        bool isBottom;
                        var constraints =
                                IntervalInference.ConstraintsFor.LessThan<IIntervalEnvironment<TVar, TExpr, TInterval, Rational>, TVar, TExpr, TInterval>
                                        (left, right, env.Decoder, env, out isBottom);
                        if (isBottom)
                                return env.Bottom;

                        var res = env;
                        foreach (var v in constraints.Keys) {
                                var intervals = constraints[v].AsEnumerable ();
                                foreach (var intv in intervals)
                                        res = res.RefineVariable (v, intv);
                        }

                        return res;
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> AssumeLessEqualThan
                        (TExpr left, TExpr right, IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> env)
                {
                        bool isBottom;
                        var constraints =
                                IntervalInference.ConstraintsFor.LessEqualThan
                                        <IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational>, TVar, TExpr, TInterval>
                                        (left, right, env.Decoder, env,
                                         out isBottom);
                        if (isBottom)
                                return env.Bottom;

                        return AssumeConstraints (constraints, env);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> AssumeGreaterEqualThanZero
                        (TExpr expr,
                         IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> env)
                {
                        var constraints =
                                IntervalInference.ConstraintsFor.GreaterEqualThanZero
                                        <IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational>, TVar, TExpr, TInterval>
                                        (expr, env.Decoder, env);
                        return AssumeConstraints (constraints, env);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> AssumeLessThan
                        (TInterval intv,
                         TVar right,
                         IntervalEnvironmentBase
                                 <TVar,
                                 TExpr,
                                 TInterval
                                 ,
                                 Rational
                                 > env)
                {
                        TInterval refined;
                        if (
                                !IntervalInference.ConstraintsFor.TryRefineLessEqualThan
                                         <IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational>, TVar, TExpr, TInterval>
                                         (intv, right, env, out refined))
                                return env;

                        return env.With (right, refined);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> AssumeLessEqualThan
                        (
                        TInterval intv, TVar right, IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> env)
                {
                        TInterval refined;
                        if (
                                !IntervalInference.ConstraintsFor.TryRefineLessEqualThan
                                         <IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational>, TVar, TExpr, TInterval>
                                         (intv, right, env, out refined))
                                return env;

                        return env.With (right, refined);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> AssumeNotEqualToZero
                        (TExpr e,
                         IntervalEnvironmentBase
                                 <
                                 TVar
                                 ,
                                 TExpr
                                 ,
                                 TInterval
                                 ,
                                 Rational
                                 >
                                 env)
                {
                        var variable = env.Decoder.UnderlyingVariable (e);

                        var intv = env.Eval (e);

                        TInterval refinement;
                        if (intv.LowerBound.IsZero)
                                refinement = env.Context.For (1L, intv.UpperBound);
                        else if (intv.UpperBound.IsZero)
                                refinement = env.Context.For (intv.LowerBound, -1L);
                        else
                                refinement = env.Context.TopValue;

                        return env.With (variable, intv.Meet (refinement));
                }
                }
}