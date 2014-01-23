// 
// ValueAnalysis.cs
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
using System.IO;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Expressions;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataFlowAnalysis;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.ExpressionAnalysis {
	class ValueAnalysis<SymbolicValue, Context, EdgeData> : IAnalysis<APC, ExprDomain<SymbolicValue>,
	                                                                 IILVisitor<APC, SymbolicValue, SymbolicValue, ExprDomain<SymbolicValue>, ExprDomain<SymbolicValue>>, EdgeData>
			where SymbolicValue : IEquatable<SymbolicValue> 
			where Context : IValueContextProvider<SymbolicValue> 
			where EdgeData : IImmutableMap<SymbolicValue, Sequence<SymbolicValue>> {
		private readonly ExpressionAnalysisFacade<SymbolicValue, Context, EdgeData> parent;

		public ValueAnalysis (ExpressionAnalysisFacade<SymbolicValue, Context, EdgeData> parent)
		{
			this.parent = parent;
		}

		#region Implementation of IAnalysis<APC,ExpressionAnalysis<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly,SymbolicValue,contextProvider,EdgeData>.Domain,IILVisitor<APC,Local,Parameter,Method,Field,Type,SymbolicValue,SymbolicValue,ExpressionAnalysis<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly,SymbolicValue,contextProvider,EdgeData>.Domain,ExpressionAnalysis<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly,SymbolicValue,contextProvider,EdgeData>.Domain>,EdgeData>
		public ExprDomain<SymbolicValue> EdgeConversion (APC @from, APC to, bool isJoinPoint, EdgeData sourceTargetMap, ExprDomain<SymbolicValue> originalState)
		{
			if (sourceTargetMap == null)
				return originalState;

			if (DebugOptions.Debug)
			{
				Console.WriteLine ("====Expression analysis Parallel assign====");
				DumpMap (sourceTargetMap);
				DumpExpressions ("original expressions", originalState);
			}

			ExprDomain<SymbolicValue> result = originalState.Empty ();
			ExprDomain<SymbolicValue> domain = originalState.Empty ();

			foreach (SymbolicValue sv in originalState.Keys) {
				Expr<SymbolicValue> expression = originalState [sv].Value.Substitute (sourceTargetMap);
				if (expression != null)
					domain = domain.Add (sv, expression);
			}

			foreach (SymbolicValue sv in sourceTargetMap.Keys) {
				FlatDomain<Expr<SymbolicValue>> expressionDomain = domain [sv];
				if (expressionDomain.IsNormal()) {
					Expr<SymbolicValue> expression = expressionDomain.Value;
					foreach (SymbolicValue sub in sourceTargetMap [sv].AsEnumerable ())
						result = result.Add (sub, expression);
				}
			}

			if (DebugOptions.Debug)
			{
				DumpExpressions ("new expressions", result);
			}
			return result;
		}

		public IILVisitor<APC, SymbolicValue, SymbolicValue, ExprDomain<SymbolicValue>, ExprDomain<SymbolicValue>> GetVisitor ()
		{
			return new AnalysisDecoder<SymbolicValue> ();
		}

		public ExprDomain<SymbolicValue> Join (Pair<APC, APC> edge, ExprDomain<SymbolicValue> newstate, ExprDomain<SymbolicValue> prevstate, out bool weaker, bool widen)
		{
			return prevstate.Join (newstate, widen, out weaker);
		}

		public ExprDomain<SymbolicValue> ImmutableVersion (ExprDomain<SymbolicValue> arg)
		{
			return arg;
		}

		public ExprDomain<SymbolicValue> MutableVersion (ExprDomain<SymbolicValue> arg)
		{
			return arg;
		}

		public bool IsBottom (APC pc, ExprDomain<SymbolicValue> state)
		{
			return state.IsBottom;
		}

		public Predicate<APC> SaveFixPointInfo (IFixPointInfo<APC, ExprDomain<SymbolicValue>> fixPointInfo)
		{
			this.parent.SaveFixPointInfo (fixPointInfo);
			return pc => true;
		}

		public void Dump (Pair<ExprDomain<SymbolicValue>, TextWriter> pair)
		{
			pair.Key.Dump (pair.Value);
		}

		private void DumpMap (IImmutableMap<SymbolicValue, Sequence<SymbolicValue>> sourceTargetMap)
		{
			Console.WriteLine ("Source-Target assignment");
			foreach (SymbolicValue key in sourceTargetMap.Keys) {
				foreach (SymbolicValue value in sourceTargetMap [key].AsEnumerable ())
					Console.Write ("{0} ", value);
				Console.WriteLine (" := {0}", key);
			}
		}

		private void DumpExpressions (string header, ExprDomain<SymbolicValue> state)
		{
			Console.WriteLine ("--- {0} ---", header);
			foreach (SymbolicValue index in state.Keys) {
				FlatDomain<Expr<SymbolicValue>> domain = state [index];
				if (domain.IsNormal())
					Console.WriteLine ("{0} -> {1}", index, domain.Value);
				else if (domain.IsTop)
					Console.WriteLine ("{0} -> (Top)", index);
				else if (domain.IsBottom)
					Console.WriteLine ("{0} -> (Bot)", index);
			}
		}
		#endregion
	}
}
