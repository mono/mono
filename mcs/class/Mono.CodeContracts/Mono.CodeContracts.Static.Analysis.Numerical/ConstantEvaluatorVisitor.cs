// 
// ConstantEvaluatorVisitor.cs
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
        abstract class ConstantEvaluatorVisitor<In, Out> {
                protected virtual bool Default (out Out result)
                {
                        return false.Without (out result);
                }

                protected bool VisitBinary (ExpressionOperator o, In left, In right, out Out result)
                {
                        try {
                                switch (o) {
                                case ExpressionOperator.And:
                                        return VisitAnd (left, right, out result);
                                case ExpressionOperator.Or:
                                        return VisitOr (left, right, out result);
                                case ExpressionOperator.Xor:
                                        return VisitXor (left, right, out result);
                                case ExpressionOperator.LogicalAnd:
                                        return VisitLogicalAnd (left, right, out result);
                                case ExpressionOperator.LogicalOr:
                                        return VisitLogicalOr (left, right, out result);
                                case ExpressionOperator.Equal:
                                case ExpressionOperator.Equal_Obj:
                                        return VisitEqual (left, right, out result);
                                case ExpressionOperator.NotEqual:
                                        return VisitNotEqual (left, right, out result);
                                case ExpressionOperator.LessThan:
                                        return VisitLessThan (left, right, out result);
                                case ExpressionOperator.LessEqualThan:
                                        return VisitLessEqualThan (left, right, out result);
                                case ExpressionOperator.GreaterThan:
                                        return VisitGreaterThan (left, right, out result);
                                case ExpressionOperator.GreaterEqualThan:
                                        return VisitGreaterEqualThan (left, right, out result);
                                case ExpressionOperator.Add:
                                        return VisitAdd (left, right, out result);
                                case ExpressionOperator.Sub:
                                        return VisitSub (left, right, out result);
                                case ExpressionOperator.Mult:
                                        return VisitMult (left, right, out result);
                                case ExpressionOperator.Div:
                                        return VisitDiv (left, right, out result);
                                case ExpressionOperator.Mod:
                                        return VisitMod (left, right, out result);
                                case ExpressionOperator.Unknown:
                                        return VisitUnknown (left, out result);
                                }
                                throw new ArgumentOutOfRangeException ("not implemented");
                        }
                        catch (ArithmeticException) {
                                return false.Without (out result);
                        }
                }

                protected virtual bool VisitMod (In left, In right, out Out result)
                {
                        return Default (out result);
                }

                protected virtual bool VisitDiv (In left, In right, out Out result)
                {
                        return Default (out result);
                }

                protected virtual bool VisitMult (In left, In right, out Out result)
                {
                        return Default (out result);
                }

                protected virtual bool VisitSub (In left, In right, out Out result)
                {
                        return Default (out result);
                }

                protected virtual bool VisitGreaterEqualThan (In left, In right, out Out result)
                {
                        return Default (out result);
                }

                protected virtual bool VisitGreaterThan (In left, In right, out Out result)
                {
                        return Default (out result);
                }

                protected virtual bool VisitLessEqualThan (In left, In right, out Out result)
                {
                        return Default (out result);
                }

                protected virtual bool VisitLessThan (In left, In right, out Out result)
                {
                        return Default (out result);
                }

                protected virtual bool VisitNotEqual (In left, In right, out Out result)
                {
                        return Default (out result);
                }

                protected virtual bool VisitEqual (In left, In right, out Out result)
                {
                        return Default (out result);
                }

                protected virtual bool VisitLogicalOr (In left, In right, out Out result)
                {
                        return Default (out result);
                }

                protected virtual bool VisitLogicalAnd (In left, In right, out Out result)
                {
                        return Default (out result);
                }

                protected virtual bool VisitXor (In left, In right, out Out result)
                {
                        return Default (out result);
                }

                protected virtual bool VisitOr (In left, In right, out Out result)
                {
                        return Default (out result);
                }

                protected virtual bool VisitAnd (In left, In right, out Out result)
                {
                        return Default (out result);
                }

                protected virtual bool VisitAdd (In left, In right, out Out result)
                {
                        return Default (out result);
                }

                protected virtual bool VisitUnknown (In left, out Out result)
                {
                        return Default (out result);
                }
        }
}