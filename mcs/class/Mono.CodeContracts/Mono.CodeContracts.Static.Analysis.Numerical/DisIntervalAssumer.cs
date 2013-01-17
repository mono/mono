// 
// DisIntervalAssumer.cs
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

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class DisIntervalAssumer<Var, Expr> : IntervalRationalAssumerBase<Var, Expr, DisInterval>
                where Var : IEquatable<Var> {
                public override IntervalEnvironmentBase<Var, Expr, DisInterval, Rational> AssumeNotEqualToZero
                        (Var var, IntervalEnvironmentBase<Var, Expr, DisInterval, Rational> env)
                {
                        return AssumeEqualToDisInterval (var, DisInterval.NotZero, env);
                }

                static IntervalEnvironmentBase<Var, Expr, DisInterval, Rational> AssumeEqualToDisInterval
                        (Var var, DisInterval intv, IntervalEnvironmentBase<Var, Expr, DisInterval, Rational> env)
                {
                        return env.RefineVariable (var, intv);
                }

                public override IntervalEnvironmentBase<Var, Expr, DisInterval, Rational> AssumeNotEqual
                        (Expr left, Expr right, IntervalEnvironmentBase<Var, Expr, DisInterval, Rational> env)
                {
                        var result = env;

                        var rightIntv = env.Eval (right);
                        if (rightIntv.IsSinglePoint) {
                                var everythingExcept = DisInterval.EverythingExcept (rightIntv);
                                result = result.RefineVariable (env.Decoder.UnderlyingVariable (left), everythingExcept);
                        }

                        IntervalInference.InferenceResult<Var, DisInterval> resultLeft;
                        IntervalInference.InferenceResult<Var, DisInterval> resultRight;
                        IntervalInference.ConstraintsFor.NotEqual (left, right, env.Decoder, result, out resultLeft,
                                                                   out resultRight);

                        var join = resultLeft.Join (resultRight);
                        if (join.IsBottom)
                                return env.Bottom as IntervalEnvironmentBase<Var, Expr, DisInterval, Rational>;

                        return AssumeConstraints (join.Constraints, env);
                }
                }
}