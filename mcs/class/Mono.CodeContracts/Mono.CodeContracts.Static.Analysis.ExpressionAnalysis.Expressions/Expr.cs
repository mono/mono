// 
// Expr.cs
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
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Expressions {
	abstract class Expr<TSymbolicValue> : IEquatable<Expr<TSymbolicValue>>
		where TSymbolicValue : IEquatable<TSymbolicValue> {
		
		public abstract IEnumerable<TSymbolicValue> Variables { get; }

		public abstract bool Equals (Expr<TSymbolicValue> other);

		public abstract Result Decode<Data, Result, Visitor> (APC pc, TSymbolicValue dest, Visitor visitor, Data data)
			where Visitor : IExpressionILVisitor<APC, TSymbolicValue, TSymbolicValue, Data, Result>;

		public abstract Expr<TSymbolicValue> Substitute (IImmutableMap<TSymbolicValue, Sequence<TSymbolicValue>> substitutions);

		/// <summary>
		/// Specifies that current expression is partially contained in candidates
		/// </summary>
		public abstract bool IsContained (IImmutableSet<TSymbolicValue> candidates);

		public abstract bool Contains (TSymbolicValue symbol);

		public abstract override string ToString ();
	}
}
