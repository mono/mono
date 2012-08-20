// 
// GetThresholdVisitor.cs
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

using System.Collections.Generic;

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class GetThresholdVisitor<TVar, TExpr> : GenericExpressionVisitor<Dummy, bool, TVar, TExpr> {
                public List<int> Thresholds { get; private set; }

                public GetThresholdVisitor (IExpressionDecoder<TVar, TExpr> decoder) : base (decoder)
                {
                        Thresholds = new List<int> ();
                }

                protected override bool Default (Dummy data)
                {
                        return false;
                }

                public override bool VisitConstant (TExpr expr, Dummy data)
                {
                        int value;
                        if (Decoder.IsConstantInt (expr, out value))
                                return false;

                        Thresholds.Add (value);
                        return true;
                }

                public override bool VisitLessThan (TExpr left, TExpr right, TExpr original, Dummy data)
                {
                        return VisitBinary (left, right, data);
                }

                public override bool VisitLessEqualThan (TExpr left, TExpr right, TExpr original, Dummy data)
                {
                        return VisitBinary (left, right, data);
                }

                public override bool VisitGreaterThan (TExpr left, TExpr right, TExpr original, Dummy data)
                {
                        return VisitBinary (left, right, data);
                }

                public override bool VisitGreaterEqualThan (TExpr left, TExpr right, TExpr original, Dummy data)
                {
                        return VisitBinary (left, right, data);
                }

                public override bool VisitNotEqual (TExpr left, TExpr right, TExpr original, Dummy data)
                {
                        return VisitBinary (left, right, data);
                }

                public override bool VisitEqual (TExpr left, TExpr right, TExpr original, Dummy data)
                {
                        return VisitBinary (left, right, data);
                }

                bool VisitBinary (TExpr left, TExpr right, Dummy data)
                {
                        var gatheredFromLeft = Visit (left, data);
                        var gatheredFromRight = Visit (right, data);

                        return gatheredFromLeft || gatheredFromRight;
                }
        }
}