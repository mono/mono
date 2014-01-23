// 
// IntervalAssumer.cs
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
        class IntervalAssumer<TVar, TExpr> : IntervalRationalAssumerBase<TVar, TExpr, Interval>
                where TVar : IEquatable<TVar> {
                public override IntervalEnvironmentBase<TVar, TExpr, Interval, Rational> AssumeNotEqual
                        (TExpr left, TExpr right, IntervalEnvironmentBase<TVar, TExpr, Interval, Rational> env)
                {
                        IntervalInference.InferenceResult<TVar, Interval> resultLeft;
                        IntervalInference.InferenceResult<TVar, Interval> resultRight;
                        IntervalInference.ConstraintsFor.NotEqual (left, right, env.Decoder, env, out resultLeft,
                                                                   out resultRight);

                        IntervalInference.InferenceResult<TVar, Interval> join = resultLeft.Join (resultRight);
                        if (join.IsBottom)
                                return env.Bottom;

                        return this.AssumeConstraints (join.Constraints, env);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, Interval, Rational> AssumeNotEqualToZero
                        (TVar v, IntervalEnvironmentBase<TVar, TExpr, Interval, Rational> env)
                {
                        //do nothing, we can't exclude one point
                        return env;
                }
         }
}