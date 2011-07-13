// 
// ExprDomain.cs
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
using System.IO;
using Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Expressions;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.ExpressionAnalysis {
	class ExprDomain<TSymValue> : IGraph<TSymValue, Dummy> 
		where TSymValue : IEquatable<TSymValue> {
		private readonly EnvironmentDomain<TSymValue, FlatDomain<Expr<TSymValue>>> expressions;

		private ExprDomain (EnvironmentDomain<TSymValue, FlatDomain<Expr<TSymValue>>> expressions)
		{
			this.expressions = expressions;
		}

		#region Implementation of IGraph<SymbolicValue,Dummy>
		public IEnumerable<TSymValue> Keys
		{
			get { return this.expressions.Keys; }
		}

		public bool IsBottom
		{
			get { return this.expressions.IsBottom; }
		}

		IEnumerable<TSymValue> IGraph<TSymValue, Dummy>.Nodes
		{
			get { return this.expressions.Keys; }
		}

		public IEnumerable<Pair<Dummy, TSymValue>> Successors(TSymValue node)
		{
			FlatDomain<Expr<TSymValue>> expr = this.expressions[node];
			if (expr.IsNormal) foreach (TSymValue sv in expr.Concrete.Variables) yield return new Pair<Dummy, TSymValue> (Dummy.Value, sv);
		}
		#endregion

		public FlatDomain<Expr<TSymValue>> this[TSymValue sv]
		{
			get { return this.expressions[sv]; }
		}

		public ExprDomain<TSymValue> Join(ExprDomain<TSymValue> that, bool widening, out bool weaker)
		{
			return new ExprDomain<TSymValue> (this.expressions.Join (that.expressions, widening, out weaker));
		}

		public static ExprDomain<TSymValue> TopValue(Func<TSymValue, int> keyConverter )
		{
			return new ExprDomain<TSymValue> (EnvironmentDomain<TSymValue, FlatDomain<Expr<TSymValue>>>.TopValue (keyConverter));
		}

		public ExprDomain<TSymValue> Add (TSymValue sv, Expr<TSymValue> expr)
		{
			return new ExprDomain<TSymValue> (this.expressions.Add (sv, expr));
		}

		public ExprDomain<TSymValue> Remove(TSymValue sv)
		{
			return new ExprDomain<TSymValue> (this.expressions.Remove (sv));
		}

		public ExprDomain<TSymValue> Empty()
		{
			return new ExprDomain<TSymValue> (this.expressions.Empty ());
		}

		public bool HasRefinement(TSymValue sv)
		{
			return this.expressions.Contains (sv);
		}

		public bool IsReachableFrom(TSymValue source, TSymValue target)
		{
			bool reachable = false;
			DepthFirst.Visit (this, source, sv => {
			                                	if (sv.Equals (target))
			                                		reachable = true;
			                                	return true;
			                                }, null);
			return reachable;
		}

		public void Dump(TextWriter tw)
		{
			this.expressions.Dump (tw);
		}
	}
}