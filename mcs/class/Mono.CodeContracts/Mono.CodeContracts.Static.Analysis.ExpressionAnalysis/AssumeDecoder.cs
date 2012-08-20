// 
// AssumeDecoder.cs
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
	struct AssumeDecoder<SymbolicValue> : IExpressionILVisitor<APC, SymbolicValue, SymbolicValue, ExprDomain<SymbolicValue>, ExprDomain<SymbolicValue>>
		where SymbolicValue : IEquatable<SymbolicValue> {
		private readonly bool truth;

		public AssumeDecoder (bool truth)
		{
			this.truth = truth;
		}

		#region IExpressionILVisitor<APC,SymbolicValue,SymbolicValue,ExprDomain<SymbolicValue>,ExprDomain<SymbolicValue>> Members
		public ExprDomain<SymbolicValue> Binary (APC pc, BinaryOperator op, SymbolicValue dest, SymbolicValue s1, SymbolicValue s2, ExprDomain<SymbolicValue> data)
		{
			if (this.truth && op.IsEqualityOperator ()) {
				if (!data.HasRefinement (s1)) {
					FlatDomain<Expr<SymbolicValue>> expression2 = data [s2];
					if (expression2.IsNormal() && !data.IsReachableFrom (s2, s1))
						return data.Add (s1, expression2.Value);
				} else if (!data.HasRefinement (s2)) {
					FlatDomain<Expr<SymbolicValue>> expression1 = data [s1];
					if (expression1.IsNormal() && !data.IsReachableFrom (s1, s2))
						return data.Add (s2, expression1.Value);
				}
			}
			if (!this.truth && op == BinaryOperator.Cne_Un) {
				if (!data.HasRefinement (s1)) {
					FlatDomain<Expr<SymbolicValue>> expression2 = data [s2];
					if (expression2.IsNormal() && !data.IsReachableFrom (s2, s1))
						return data.Add (s1, expression2.Value);
				} else if (!data.HasRefinement (s2)) {
					FlatDomain<Expr<SymbolicValue>> expression1 = data [s1];
					if (expression1.IsNormal() && !data.IsReachableFrom (s1, s2))
						return data.Add (s2, expression1.Value);
				}
			}
			return data;
		}

		public ExprDomain<SymbolicValue> Isinst (APC pc, TypeNode type, SymbolicValue dest, SymbolicValue obj, ExprDomain<SymbolicValue> data)
		{
			return data;
		}

		public ExprDomain<SymbolicValue> LoadNull (APC pc, SymbolicValue dest, ExprDomain<SymbolicValue> polarity)
		{
			return polarity;
		}

		public ExprDomain<SymbolicValue> LoadConst (APC pc, TypeNode type, object constant, SymbolicValue dest, ExprDomain<SymbolicValue> data)
		{
			return data;
		}

		public ExprDomain<SymbolicValue> Sizeof (APC pc, TypeNode type, SymbolicValue dest, ExprDomain<SymbolicValue> data)
		{
			return data;
		}

		public ExprDomain<SymbolicValue> Unary (APC pc, UnaryOperator op, bool unsigned, SymbolicValue dest, SymbolicValue source, ExprDomain<SymbolicValue> data)
		{
			return data;
		}
		#endregion
	}
}
