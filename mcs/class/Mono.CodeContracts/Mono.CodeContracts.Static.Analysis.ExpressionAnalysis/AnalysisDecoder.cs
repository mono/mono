// 
// AnalysisDecoder.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Expressions;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.ExpressionAnalysis {
	class AnalysisDecoder<TSymValue> : ILVisitorBase<APC, TSymValue, TSymValue, ExprDomain<TSymValue>, ExprDomain<TSymValue>>
		where TSymValue : IEquatable<TSymValue> {
		public override ExprDomain<TSymValue> DefaultVisit (APC pc, ExprDomain<TSymValue> data)
		{
			return data;
		}

		public override ExprDomain<TSymValue> Assume (APC pc, EdgeTag tag, TSymValue condition, ExprDomain<TSymValue> data)
		{
			FlatDomain<Expr<TSymValue>> aExpression = data [condition];

			if (aExpression.IsNormal()) {
				bool truth = tag != EdgeTag.False;
				data = aExpression.Value.Decode<ExprDomain<TSymValue>, ExprDomain<TSymValue>, AssumeDecoder<TSymValue>>
					(pc, condition, new AssumeDecoder<TSymValue> (truth), data);
			}

			return data;
		}

		public override ExprDomain<TSymValue> Assert (APC pc, EdgeTag tag, TSymValue condition, ExprDomain<TSymValue> data)
		{
			FlatDomain<Expr<TSymValue>> expression = data [condition];
			if (expression.IsNormal()) {
				data = expression.Value.Decode<ExprDomain<TSymValue>, ExprDomain<TSymValue>, AssumeDecoder<TSymValue>>
					(pc, condition, new AssumeDecoder<TSymValue> (true), data);
			}

			return data;
		}

		public override ExprDomain<TSymValue> Binary (APC pc, BinaryOperator op, TSymValue dest, TSymValue operand1, TSymValue operand2, ExprDomain<TSymValue> data)
		{
			return data.Add (dest, new BinaryExpr<TSymValue> (operand1, operand2, op));
		}

		public override ExprDomain<TSymValue> Isinst (APC pc, TypeNode type, TSymValue dest, TSymValue obj, ExprDomain<TSymValue> data)
		{
			return data.Add (dest, new IsInstExpr<TSymValue> (obj, type));
		}

		public override ExprDomain<TSymValue> LoadConst (APC pc, TypeNode type, object constant, TSymValue dest, ExprDomain<TSymValue> data)
		{
			return data.Add (dest, new ConstExpr<TSymValue> (type, constant));
		}

		public override ExprDomain<TSymValue> LoadNull (APC pc, TSymValue dest, ExprDomain<TSymValue> polarity)
		{
			return polarity.Add (dest, NullExpr<TSymValue>.Instance);
		}

		public override ExprDomain<TSymValue> Sizeof (APC pc, TypeNode type, TSymValue dest, ExprDomain<TSymValue> data)
		{
			return data.Add (dest, new SizeOfExpr<TSymValue> (type));
		}

		public override ExprDomain<TSymValue> Unary (APC pc, UnaryOperator op, bool unsigned, TSymValue dest, TSymValue source, ExprDomain<TSymValue> data)
		{
			return data.Add (dest, new UnaryExpr<TSymValue> (source, op, unsigned));
		}
	}
}
