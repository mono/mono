// 
// ExpressionAnalysisFacade.cs
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
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataFlowAnalysis;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis.ExpressionAnalysis {
	class ExpressionAnalysisFacade<TSymValue, TContext, TEdgeData>
		where TSymValue : IEquatable<TSymValue>
		where TContext : IValueContextProvider<TSymValue>
		where TEdgeData : IImmutableMap<TSymValue, LispList<TSymValue>> {
		
		public readonly Predicate<APC> IsUnreachable;

		public readonly ICodeLayer<TSymValue, TSymValue, TContext, TEdgeData> ValueLayer;
		private IFixPointInfo<APC, ExprDomain<TSymValue>> fix_point_info;

		public ExpressionAnalysisFacade (ICodeLayer<TSymValue, TSymValue, TContext, TEdgeData> valueLayer,
		                                 Predicate<APC> isUnreachable)
		{
			this.ValueLayer = valueLayer;
			this.IsUnreachable = isUnreachable;
		}

		public bool PreStateLookup (APC label, out ExprDomain<TSymValue> ifFound)
		{
			return this.fix_point_info.PreStateLookup (label, out ifFound);
		}

		public bool PostStateLookup (APC label, out ExprDomain<TSymValue> ifFound)
		{
			return this.fix_point_info.PostStateLookup (label, out ifFound);
		}

		public void SaveFixPointInfo (IFixPointInfo<APC, ExprDomain<TSymValue>> fixPointInfo)
		{
			this.fix_point_info = fixPointInfo;
		}

		public ExprDomain<TSymValue> InitialValue (Func<TSymValue, int> keyConverter)
		{
			return ExprDomain<TSymValue>.TopValue (keyConverter);
		}

		public IAnalysis<APC, ExprDomain<TSymValue>, IILVisitor<APC, TSymValue, TSymValue, ExprDomain<TSymValue>, ExprDomain<TSymValue>>, TEdgeData> 
			CreateExpressionAnalysis ()
		{
			return new ValueAnalysis<TSymValue, TContext, TEdgeData> (this);
		}

		public IILDecoder<APC, LabeledSymbol<APC, TSymValue>, TSymValue, IExpressionContextProvider<LabeledSymbol<APC, TSymValue>, TSymValue>, TEdgeData> 
			GetDecoder (
			IILDecoder<APC, TSymValue, TSymValue, IValueContextProvider<TSymValue>, TEdgeData> ilDecoder)
		{
			return new ExpressionDecoder<TSymValue, TContext, TEdgeData> (ilDecoder, this);
		}
	}
}
