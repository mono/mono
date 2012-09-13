// 
// ExpressionDecoder.cs
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
using Mono.CodeContracts.Static.Analysis.HeapAnalysis;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis.ExpressionAnalysis {
	class ExpressionDecoder<TSymbolicValue, TContext, TEdgeData> :
		IILDecoder<APC, LabeledSymbol<APC, TSymbolicValue>, TSymbolicValue, IExpressionContextProvider<LabeledSymbol<APC, TSymbolicValue>, TSymbolicValue>, TEdgeData>,
		IExpressionContextProvider<LabeledSymbol<APC, TSymbolicValue>, TSymbolicValue>,
		IExpressionContext<LabeledSymbol<APC, TSymbolicValue>, TSymbolicValue>
		where TSymbolicValue : IEquatable<TSymbolicValue>
		where TContext : IValueContextProvider<TSymbolicValue>
		where TEdgeData : IImmutableMap<TSymbolicValue, Sequence<TSymbolicValue>> {
		private readonly IILDecoder<APC, TSymbolicValue, TSymbolicValue, IValueContextProvider<TSymbolicValue>, TEdgeData> value_decoder;
		private readonly ExpressionAnalysisFacade<TSymbolicValue, TContext, TEdgeData> parent;
		private readonly IValueContextProvider<TSymbolicValue> underlying;

		public ExpressionDecoder (IILDecoder<APC, TSymbolicValue, TSymbolicValue, IValueContextProvider<TSymbolicValue>, TEdgeData> valueDecoder,
		                          ExpressionAnalysisFacade<TSymbolicValue, TContext, TEdgeData> parent)
		{
			this.value_decoder = valueDecoder;
			this.parent = parent;
			this.underlying = valueDecoder.ContextProvider;
		}

		#region IExpressionContext<LabeledSymbol<APC,SymbolicValue>,SymbolicValue> Members
		public LabeledSymbol<APC, TSymbolicValue> Refine (APC pc, TSymbolicValue variable)
		{
			return new LabeledSymbol<APC, TSymbolicValue> (pc, variable);
		}

		public TSymbolicValue Unrefine (LabeledSymbol<APC, TSymbolicValue> expression)
		{
			return expression.Symbol;
		}

		public Result Decode<Data, Result, Visitor> (LabeledSymbol<APC, TSymbolicValue> expr, Visitor visitor, Data data)
			where Visitor : ISymbolicExpressionVisitor<LabeledSymbol<APC, TSymbolicValue>, LabeledSymbol<APC, TSymbolicValue>, TSymbolicValue, Data, Result>
		{
			ExprDomain<TSymbolicValue> ifFound;
			if (!this.parent.PreStateLookup (expr.ReadAt, out ifFound) || ifFound.IsBottom)
				return visitor.SymbolicConstant (expr, expr.Symbol, data);

			FlatDomain<Expr<TSymbolicValue>> aExpr = ifFound [expr.Symbol];
			if (aExpr.IsNormal()) {
				return aExpr.Value.Decode<Data, Result, ExpressionDecoderAdapter<TSymbolicValue, Data, Result, Visitor>>
					(expr.ReadAt, expr.Symbol, new ExpressionDecoderAdapter<TSymbolicValue, Data, Result, Visitor> (visitor), data);
			}

			TypeNode type;
			object constant;
			if (this.parent.ValueLayer.ILDecoder.ContextProvider.ValueContext.IsConstant (expr.ReadAt, expr.Symbol, out type, out constant))
				return visitor.LoadConst (expr, type, constant, expr.Symbol, data);

			return visitor.SymbolicConstant (expr, expr.Symbol, data);
		}

		public FlatDomain<TypeNode> GetType (LabeledSymbol<APC, TSymbolicValue> expr)
		{
			return this.underlying.ValueContext.GetType (expr.ReadAt, expr.Symbol);
		}

		public APC GetPC (LabeledSymbol<APC, TSymbolicValue> pc)
		{
			return pc.ReadAt;
		}

		public LabeledSymbol<APC, TSymbolicValue> For (TSymbolicValue variable)
		{
			return new LabeledSymbol<APC, TSymbolicValue> (MethodContext.CFG.Entry, variable);
		}

		public bool IsZero (LabeledSymbol<APC, TSymbolicValue> expression)
		{
			return ValueContext.IsZero (expression.ReadAt, expression.Symbol);
		}
		#endregion

		#region IILDecoder<APC,LabeledSymbol<APC,SymbolicValue>,SymbolicValue,IExpressionContextProvider<LabeledSymbol<APC,SymbolicValue>,SymbolicValue>,EdgeData> Members
		public IExpressionContextProvider<LabeledSymbol<APC, TSymbolicValue>, TSymbolicValue> ContextProvider
		{
			get { return this; }
		}

		public Result ForwardDecode<Data, Result, Visitor> (APC pc, Visitor visitor, Data state)
			where Visitor : IILVisitor<APC, LabeledSymbol<APC, TSymbolicValue>, TSymbolicValue, Data, Result>
		{
			return this.value_decoder.ForwardDecode<Data, Result, ILDecoderAdapter<TSymbolicValue, Data, Result, Visitor>>
				(pc, new ILDecoderAdapter<TSymbolicValue, Data, Result, Visitor> (visitor), state);
		}

		public bool IsUnreachable (APC pc)
		{
			return this.parent.IsUnreachable (pc);
		}

		public TEdgeData EdgeData (APC from, APC to)
		{
			return this.parent.ValueLayer.ILDecoder.EdgeData (from, to);
		}
		#endregion

		#region Implementation of IMethodContextProvider<Field,Method>
		public IMethodContext MethodContext
		{
			get { return this.underlying.MethodContext; }
		}
		#endregion

		#region Implementation of IStackContextProvider<Field,Method>
		public IStackContext StackContext
		{
			get { return this.underlying.StackContext; }
		}
		#endregion

		#region Implementation of IValueContextProvider<Local,Parameter,Method,Field,Type,SymbolicValue>
		public IValueContext<TSymbolicValue> ValueContext
		{
			get { return this.underlying.ValueContext; }
		}
		#endregion

		#region Implementation of IExpressionContextProvider<Local,Parameter,Method,Field,Type,ExternalExpression<APC,SymbolicValue>,SymbolicValue>
		public IExpressionContext<LabeledSymbol<APC, TSymbolicValue>, TSymbolicValue> ExpressionContext
		{
			get { return this; }
		}
		#endregion
	}
}
