// 
// BinaryExpr.cs
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
using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Expressions {
	class BinaryExpr<TSymbolicValue> : Expr<TSymbolicValue>
		where TSymbolicValue : IEquatable<TSymbolicValue> {
		
		public readonly BinaryOperator Operator;
		public readonly TSymbolicValue Left;
		public readonly TSymbolicValue Right;

		public BinaryExpr (TSymbolicValue left, TSymbolicValue right, BinaryOperator op)
		{
			this.Left = left;
			this.Right = right;
			this.Operator = op;
		}

		#region Overrides of Expression
		public override IEnumerable<TSymbolicValue> Variables
		{
			get
			{
				yield return this.Left;
				yield return this.Right;
			}
		}

		public override Result Decode<Data, Result, Visitor> (APC pc, TSymbolicValue dest, Visitor visitor, Data data)
		{
			return visitor.Binary (pc, this.Operator, dest, this.Left, this.Right, data);
		}

		public override Expr<TSymbolicValue> Substitute (IImmutableMap<TSymbolicValue, Sequence<TSymbolicValue>> substitutions)
		{
			if (substitutions.ContainsKey (this.Left) && substitutions.ContainsKey (this.Right))
				return new BinaryExpr<TSymbolicValue> (substitutions [this.Left].Head, substitutions [this.Right].Head, this.Operator);

			return null;
		}

		public override bool IsContained (IImmutableSet<TSymbolicValue> candidates)
		{
			return candidates.Contains (this.Left) || candidates.Contains (this.Right);
		}

		public override bool Contains (TSymbolicValue symbol)
		{
			return this.Left.Equals (symbol) || this.Right.Equals (symbol);
		}

		public override string ToString ()
		{
			return String.Format ("Binary({0} {1} {2})", this.Left, this.Operator, this.Right);
		}

		public override bool Equals (Expr<TSymbolicValue> other)
		{
			var binary = other as BinaryExpr<TSymbolicValue>;
			if (binary == null || binary.Operator != this.Operator)
				return false;

			return binary.Left.Equals (this.Left) && binary.Right.Equals (this.Right);
		}
		#endregion
	}
}
