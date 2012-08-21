// 
// AssumeTrueVisitor.cs
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

using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        abstract class AssumeTrueVisitor<TDomain, TVar, TExpr> : GenericExpressionVisitor<TDomain, TDomain, TVar, TExpr>
                where TDomain : IEnvironmentDomain<TDomain, TVar, TExpr> {
                protected AssumeTrueVisitor (IExpressionDecoder<TVar, TExpr> decoder)
                        : base (decoder)
                {
                }

                public AssumeFalseVisitor<TDomain, TVar, TExpr> FalseVisitor { get; set; }

                protected override TDomain Default (TDomain data)
                {
                        return data;
                }

                public override TDomain VisitConstant (TExpr left, TDomain data)
                {
                        bool boolValue;
                        if (Decoder.TryValueOf (left, ExpressionType.Bool, out boolValue))
                                return boolValue ? data : data.Bottom;

                        int intValue;
                        if (Decoder.TryValueOf (left, ExpressionType.Int32, out intValue))
                                return intValue != 0 ? data : data.Bottom;

                        return data;
                }

                public override TDomain VisitLogicalAnd (TExpr left, TExpr right, TExpr original, TDomain data)
                {
                        var leftIsVariable = Decoder.IsVariable (left);
                        var rightIsVariable = Decoder.IsVariable (right);

                        var leftIsConstant = Decoder.IsConstant (left);
                        var rightIsConstant = Decoder.IsConstant (right);

                        if (leftIsVariable && rightIsConstant || leftIsConstant && rightIsVariable)
                                return data;

                        return data.AssumeTrue (left).AssumeTrue (right);
                }

                public override TDomain VisitLogicalOr (TExpr left, TExpr right, TExpr original, TDomain data)
                {
                        var leftIsVariable = Decoder.IsVariable (left);
                        var rightIsVariable = Decoder.IsVariable (right);

                        var leftIsConstant = Decoder.IsConstant (left);
                        var rightIsConstant = Decoder.IsConstant (right);

                        if (leftIsVariable && rightIsConstant || leftIsConstant && rightIsVariable)
                                return data;

                        var leftBranch = data.AssumeTrue (left);
                        var rightBranch = data.AssumeTrue (right);

                        return leftBranch.Join (rightBranch);
                }

                public override TDomain VisitNot (TExpr expr, TDomain data)
                {
                        return FalseVisitor.Visit (expr, data);
                }

                protected override bool TryPolarity (TExpr expr, TDomain data, out bool shouldNegate)
                {
                        if (base.TryPolarity (expr, data, out shouldNegate))
                                return true;

                        var holds = data.CheckIfHolds (expr);
                        if (!holds.IsNormal ())
                                return false.Without (out shouldNegate);

                        return true.With (!holds.Value, out shouldNegate);
                }
                }
}