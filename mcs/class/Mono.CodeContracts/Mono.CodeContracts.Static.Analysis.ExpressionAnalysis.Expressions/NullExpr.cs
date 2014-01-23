// 
// NullExpr.cs
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
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Expressions {
	sealed class NullExpr<TSymbolicValue> : Expr<TSymbolicValue> where TSymbolicValue : IEquatable<TSymbolicValue> {
		public static readonly Expr<TSymbolicValue> Instance = new NullExpr<TSymbolicValue> ();

		private NullExpr ()
		{
		}

		#region Overrides of Expression
		public override IEnumerable<TSymbolicValue> Variables
		{
			get { yield break; }
		}

		public override Result Decode<Data, Result, Visitor> (APC pc, TSymbolicValue dest, Visitor visitor, Data data)
		{
			return visitor.LoadNull (pc, dest, data);
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
			return "Null()";
		}

		public override bool Equals (Expr<TSymbolicValue> other)
		{
			return other == this;
		}
		#endregion
	}
}
