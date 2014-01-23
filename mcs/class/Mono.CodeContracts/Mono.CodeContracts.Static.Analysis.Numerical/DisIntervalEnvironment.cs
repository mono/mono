// 
// DisIntervalEnvironment.cs
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

using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class DisIntervalEnvironment<TVar, TExpr> :
                IntervalEnvironmentBase<TVar, TExpr, DisInterval, Rational> where TVar : IEquatable<TVar> {
                static DisIntervalAssumer<TVar, TExpr> cached_assumer;

                DisIntervalEnvironment (IExpressionDecoder<TVar, TExpr> decoder,
                                        EnvironmentDomain<TVar, DisInterval> varsToInterval)
                        : base (decoder, varsToInterval)
                {
                }

                public DisIntervalEnvironment (IExpressionDecoder<TVar, TExpr> decoder)
                        : base (decoder)
                {
                }

                public override IntervalAssumerBase<TVar, TExpr, DisInterval, Rational> Assumer
                {
                        get
                        {
                                return cached_assumer ??
                                       (cached_assumer = new DisIntervalAssumer<TVar, TExpr> ());
                        }
                }

                public override IntervalContextBase<DisInterval, Rational> Context { get { return DisIntervalContext.Instance; } }

                protected override IntervalEnvironmentBase<TVar, TExpr, DisInterval, Rational> NewInstance (
                        EnvironmentDomain<TVar, DisInterval> varsToIntervals)
                {
                        return new DisIntervalEnvironment<TVar, TExpr> (Decoder, varsToIntervals);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, DisInterval, Rational> AssumeVariableIn (TVar var,
                                                                                                              Interval
                                                                                                                      interval)
                {
                        return RefineVariable (var, DisInterval.For (interval));
                }
                }
}