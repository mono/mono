// 
// SizeoOfExpr.cs
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
using System.Linq;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Expressions {
	sealed class SizeOfExpr<TSymbolicValue> : Expr<TSymbolicValue> where TSymbolicValue : IEquatable<TSymbolicValue> {

		public readonly TypeNode Type;

		public SizeOfExpr (TypeNode type)
		{
			this.Type = type;
		}

		#region Overrides of Expression
		public override IEnumerable<TSymbolicValue> Variables
		{
			get { return Enumerable.Empty<TSymbolicValue> (); }
		}

		public override Result Decode<Data, Result, Visitor> (APC pc, TSymbolicValue dest, Visitor visitor, Data data)
		{
			return visitor.Sizeof (pc, this.Type, dest, data);
		}

		public override Expr<TSymbolicValue> Substitute (IImmutableMap<TSymbolicValue, Sequence<TSymbolicValue>> substitutions)
		{
			return this;
		}

		public override bool IsContained (IImmutableSet<TSymbolicValue> candidates)
		{
			return false;
		}

		public override bool Contains (TSymbolicValue symbol)
		{
			return false;
		}

		public override string ToString ()
		{
			return String.Format ("Sizeof({0})", this.Type);
		}

		public override bool Equals (Expr<TSymbolicValue> other)
		{
			var @sizeof = other as SizeOfExpr<TSymbolicValue>;

			return (@sizeof != null && @sizeof.Type.Equals (this.Type));
		}
		#endregion
	}
}