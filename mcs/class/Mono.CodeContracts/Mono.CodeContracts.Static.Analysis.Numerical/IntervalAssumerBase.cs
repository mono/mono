// 
// IntervalAssumerBase.cs
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
        abstract class IntervalAssumerBase<TVar, TExpr, TInterval, TNumeric>
                where TInterval : IntervalBase<TInterval, TNumeric>
                where TVar : IEquatable<TVar> {
                public virtual IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> AssumeEqual
                        (TExpr left,
                         TExpr right,
                         IntervalEnvironmentBase
                                 <TVar,
                                 TExpr,
                                 TInterval,
                                 TNumeric>
                                 env)
                {
                        var leftVar = env.Decoder.UnderlyingVariable (left);
                        var rightVar = env.Decoder.UnderlyingVariable (right);

                        if (env.Contains (leftVar)) {
                                var res = env;
                                var interval = env.Eval (left).Meet (env.Eval (right));

                                res = res.With (leftVar, interval);
                                res = res.With (rightVar, interval);

                                return res;
                        }

                        if (env.Decoder.IsConstant (left) && env.Decoder.IsConstant (right) &&
                            env.Eval (left).Meet (env.Eval (right)).IsBottom)
                                return env.Bottom;

                        return env;
                }

                public virtual IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> AssumeEqualToZero
                        (TVar var,
                         IntervalEnvironmentBase
                                 <
                                 TVar
                                 ,
                                 TExpr
                                 ,
                                 TInterval
                                 ,
                                 TNumeric
                                 >
                                 env)
                {
                        return env.RefineVariable (var, env.Context.Zero);
                }

                public virtual IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> AssumeNotEqual
                        (TExpr left,
                         TExpr right,
                         IntervalEnvironmentBase
                                 <TVar,
                                 TExpr,
                                 TInterval
                                 ,
                                 TNumeric
                                 > env)
                {
                        int value;
                        if (env.Decoder.OperatorFor (left).IsRelational () && env.Decoder.IsConstantInt (right, out value))
                                return value == 0 ? env.AssumeTrue (left) : env.AssumeFalse (left);

                        var assumer = env.Assumer;
                        return assumer.AssumeLessThan (left, right, env).Join (assumer.AssumeLessThan (right, left, env));
                }

                public abstract IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> AssumeLessThan
                        (TExpr left,
                         TExpr right,
                         IntervalEnvironmentBase
                                 <TVar,
                                 TExpr,
                                 TInterval
                                 ,
                                 TNumeric
                                 > env);

                public abstract IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> AssumeLessEqualThan
                        (
                        TExpr left, TExpr right, IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> env);

                public abstract IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> AssumeNotEqualToZero
                        (TVar v, IntervalEnvironmentBase<TVar,TExpr,TInterval,TNumeric> env);

                public abstract IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> AssumeNotEqualToZero
                        (TExpr e, IntervalEnvironmentBase<TVar,TExpr,TInterval,TNumeric> env);

                public abstract IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> AssumeLessEqualThan
                        (TInterval intv, TVar right, IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> env);

                public abstract IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> AssumeLessThan
                        (TInterval intv, TVar right, IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> env);

                public abstract IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> AssumeGreaterEqualThanZero
                        (TExpr expr, IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> env);

                protected IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> AssumeConstraints
                        (IImmutableMap<TVar, Sequence<TInterval>> constraints,
                        IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> env)
                {
                        var res = env;
                        foreach (var v in constraints.Keys) {
                                var seq = constraints[v];
                                foreach (var intv in seq.AsEnumerable ())
                                        res = res.RefineVariable (v, intv);
                        }

                        return res;
                }

                protected IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> AssumeConstraints
                        (
                        IDictionary<TVar, Sequence<TInterval>> constraints,
                        IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> env)
                {
                        var res = env;
                        foreach (var v in constraints.Keys) {
                                var seq = constraints[v];
                                foreach (var intv in seq.AsEnumerable ())
                                        res = res.RefineVariable (v, intv);
                        }

                        return res;
                }
        }
}