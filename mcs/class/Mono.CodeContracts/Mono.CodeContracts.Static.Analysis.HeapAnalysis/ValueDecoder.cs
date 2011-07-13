// 
// ValueDecoder.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
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

using System;
using System.IO;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis {
	class ValueDecoder<TContext> : 
		IILDecoder<APC, SymbolicValue, SymbolicValue, IValueContextProvider<SymbolicValue>, IImmutableMap<SymbolicValue, LispList<SymbolicValue>>>
		where TContext : IStackContextProvider
	{
		private readonly HeapAnalysis parent;
		private readonly IILDecoder<APC, int, int, TContext, Dummy> stack_decoder;
		private readonly Lazy<IValueContextProvider<SymbolicValue>> context;

		public ValueDecoder(HeapAnalysis parent, IILDecoder<APC, int, int, TContext, Dummy> stackDecoder)
		{
			this.context = new Lazy<IValueContextProvider<SymbolicValue>> (()=> new ValueContextProvider<TContext>(this.parent, this.stack_decoder.ContextProvider));
			this.parent = parent;
			this.stack_decoder = stackDecoder;
		}

		#region Implementation of IILDecoder<APC,SymbolicValue,SymbolicValue,IValueContext<SymbolicValue>,IImmutableMap<SymbolicValue,LispList<SymbolicValue>>>
		public IValueContextProvider<SymbolicValue> ContextProvider { get { return context.Value; } }

		public Result ForwardDecode<Data, Result, Visitor>(APC pc, Visitor visitor, Data state)
			where Visitor : IILVisitor<APC, SymbolicValue, SymbolicValue, Data, Result>
		{
			return this.stack_decoder.ForwardDecode<Data, Result, StackToSymbolicAdapter<Data, Result, Visitor>> 
				(pc, new StackToSymbolicAdapter<Data, Result, Visitor> (this.parent, visitor), state);
		}

		public bool IsUnreachable(APC pc)
		{
			return this.parent.IsUnreachable (pc);
		}

		public IImmutableMap<SymbolicValue, LispList<SymbolicValue>> EdgeData(APC from, APC to)
		{
			if (!this.parent.RenamePoints.ContainsKey(from, to))
				return null;
			if (this.parent.MergeInfoCache.ContainsKey(to) && this.parent.MergeInfoCache[to] == null)
				return null;

			return this.parent.EdgeRenaming (new Pair<APC, APC> (from, to), this.ContextProvider.MethodContext.CFG.IsJoinPoint (to));
		}

		public void Dump(TextWriter tw, string prefix, IImmutableMap<SymValue, LispList<SymValue>> edgeData )
		{
			if (edgeData == null)
				return;
			edgeData.Visit ((key, targets) => {
			                	tw.Write ("  {0} -> ", key);
			                	foreach (var target in targets.AsEnumerable ())
			                		tw.Write ("{0} ", target);
			                	tw.WriteLine ();
			                	return VisitStatus.ContinueVisit;
			                });
		}

		#endregion
	}
}