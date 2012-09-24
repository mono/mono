// 
// IIntervalEnvironment.cs
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

using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        interface INumericalEnvironmentDomain<TVar, TExpr> :
                IEnvironmentDomain<INumericalEnvironmentDomain<TVar, TExpr>, TVar, TExpr> {
                INumericalEnvironmentDomain<TVar, TExpr> AssumeVariableIn (TVar var, Interval interval);
                INumericalEnvironmentDomain<TVar, TExpr> AssumeLessEqualThan (TExpr left, TExpr right);
                }

        interface IIntervalEnvironment<TVar, TExpr, TInterval, TNumeric> : INumericalEnvironmentDomain<TVar, TExpr>
                where TInterval : IntervalBase<TInterval, TNumeric> {
                IntervalContextBase<TInterval, TNumeric> Context { get; }

                TInterval Eval (TExpr expr);
                TInterval Eval (TVar expr);

                bool TryGetValue (TVar rightVar, out TInterval intv);
                }

        static class NumericalEnvironmentDomainExtensions {
                public static INumericalEnvironmentDomain<TVar, TExpr> AssumeInInterval<TVar, TExpr> (
                        this INumericalEnvironmentDomain<TVar, TExpr> domain, TExpr expr, Interval intv,
                        IExpressionEncoder<TVar, TExpr> encoder)
                {
                        if (!domain.IsNormal ())
                                return domain;

                        if (intv.IsBottom)
                                return domain.Bottom;

                        if (!intv.LowerBound.IsInfinity)
                                domain = domain.AssumeLessEqualThan (intv.LowerBound.ToExpression (encoder), expr);

                        if (!intv.UpperBound.IsInfinity)
                                domain = domain.AssumeLessEqualThan (expr, intv.LowerBound.ToExpression (encoder));

                        return domain;
                }
        }
}