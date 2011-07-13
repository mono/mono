// 
// ContractFilteredCFG.cs
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
// 

using System;
using System.Collections.Generic;
using System.IO;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.ControlFlow {
	class ContractFilteredCFG : ICFG, IEdgeSubroutineAdaptor {
		private readonly ICFG underlying;

		public ContractFilteredCFG (ICFG cfg)
		{
			this.underlying = cfg;
		}

		#region ICFG Members
		public APC Entry
		{
			get { return this.underlying.Entry; }
		}

		public APC EntryAfterRequires
		{
			get { return this.underlying.EntryAfterRequires; }
		}

		public APC NormalExit
		{
			get { return this.underlying.NormalExit; }
		}

		public APC ExceptionExit
		{
			get { return this.underlying.ExceptionExit; }
		}

		public Subroutine Subroutine
		{
			get { return this.underlying.Subroutine; }
		}

		public APC Next (APC pc)
		{
			return this.underlying.Next (pc);
		}

		public bool HasSingleSuccessor (APC pc, out APC ifFound)
		{
			DecoratorHelper.Push<IEdgeSubroutineAdaptor> (this);
			try {
				return this.underlying.HasSingleSuccessor (pc, out ifFound);
			} finally {
				DecoratorHelper.Pop ();
			}
		}

		public IEnumerable<APC> Successors (APC pc)
		{
			DecoratorHelper.Push<IEdgeSubroutineAdaptor> (this);
			try {
				return this.underlying.Successors (pc);
			} finally {
				DecoratorHelper.Pop ();
			}
		}

		public bool HasSinglePredecessor (APC pc, out APC ifFound)
		{
			DecoratorHelper.Push<IEdgeSubroutineAdaptor> (this);
			try {
				return this.underlying.HasSinglePredecessor (pc, out ifFound);
			} finally {
				DecoratorHelper.Pop ();
			}
		}

		public IEnumerable<APC> Predecessors (APC pc)
		{
			DecoratorHelper.Push<IEdgeSubroutineAdaptor> (this);
			try {
				return this.underlying.Predecessors (pc);
			} finally {
				DecoratorHelper.Pop ();
			}
		}

		public bool IsJoinPoint (APC pc)
		{
			return this.underlying.IsJoinPoint (pc);
		}

		public bool IsSplitPoint (APC pc)
		{
			return this.underlying.IsSplitPoint (pc);
		}

		public bool IsBlockStart (APC pc)
		{
			return this.underlying.IsBlockStart (pc);
		}

		public bool IsBlockEnd (APC pc)
		{
			return this.underlying.IsBlockEnd (pc);
		}

		public IILDecoder<APC, Dummy, Dummy, IMethodContextProvider, Dummy> GetDecoder (IMetaDataProvider metaDataProvider)
		{
			return this.underlying.GetDecoder (metaDataProvider);
		}

		public void Print (TextWriter tw, ILPrinter<APC> printer,
		                   Func<CFGBlock, IEnumerable<LispList<Edge<CFGBlock, EdgeTag>>>> contextLookup,
		                   LispList<Edge<CFGBlock, EdgeTag>> context)
		{
			DecoratorHelper.Push<IEdgeSubroutineAdaptor> (this);
			try {
				this.underlying.Print (tw, printer, contextLookup, context);
			} finally {
				DecoratorHelper.Pop ();
			}
		}
		#endregion

		#region Implementation of IEdgeSubroutineAdaptor
		LispList<Pair<EdgeTag, Subroutine>> IEdgeSubroutineAdaptor.GetOrdinaryEdgeSubroutinesInternal (CFGBlock @from, CFGBlock to,
		                                                                                               LispList<Edge<CFGBlock, EdgeTag>> context)
		{
			return DecoratorHelper.Inner<IEdgeSubroutineAdaptor> (this)
				.GetOrdinaryEdgeSubroutinesInternal (from, to, context).Where ((pair) => !pair.Value.IsContract && !pair.Value.IsOldValue);
		}
		#endregion
	}
}
