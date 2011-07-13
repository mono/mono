// 
// SubroutineWithHandlers.cs
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
using System.Linq;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.ControlFlow.Blocks;
using Mono.CodeContracts.Static.ControlFlow.Subroutines.Builders;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines {
	abstract class SubroutineWithHandlers<Label, Handler> : SubroutineBase<Label> {
		protected readonly Dictionary<Handler, BlockWithLabels<Label>> CatchFilterHeaders = new Dictionary<Handler, BlockWithLabels<Label>> ();

		public readonly Dictionary<Handler, Subroutine> FaultFinallySubroutines = new Dictionary<Handler, Subroutine> ();
		protected readonly Dictionary<Handler, BlockWithLabels<Label>> FilterCodeBlocks = new Dictionary<Handler, BlockWithLabels<Label>> ();
		public readonly Dictionary<CFGBlock, LispList<Handler>> ProtectingHandlers = new Dictionary<CFGBlock, LispList<Handler>> ();
		public LispList<Handler> CurrentProtectingHandlers = LispList<Handler>.Empty;

		protected SubroutineWithHandlers (SubroutineFacade subroutineFacade)
			: base (subroutineFacade)
		{
		}

		protected SubroutineWithHandlers (SubroutineFacade subroutineFacade,
		                                  Label startLabel,
		                                  SubroutineBuilder<Label> builder)
			: base (subroutineFacade, startLabel, builder)
		{
		}

		protected new IMethodCodeProvider<Label, Handler> CodeProvider
		{
			get { return (IMethodCodeProvider<Label, Handler>) base.CodeProvider; }
		}

		private bool IsFault (Handler handler)
		{
			return CodeProvider.IsFaultHandler (handler);
		}

		private LispList<Handler> ProtectingHandlerList (CFGBlock block)
		{
			LispList<Handler> list;
			this.ProtectingHandlers.TryGetValue (block, out list);
			return list;
		}

		public BlockWithLabels<Label> CreateCatchFilterHeader (Handler handler, Label label)
		{
			BlockWithLabels<Label> block;
			if (!this.LabelsThatStartBlocks.TryGetValue (label, out block)) {
				block = new CatchFilterEntryBlock<Label> (this, ref this.BlockIdGenerator);

				this.CatchFilterHeaders.Add (handler, block);
				this.LabelsThatStartBlocks.Add (label, block);
				if (CodeProvider.IsFilterHandler (handler)) {
					BlockWithLabels<Label> targetBlock = GetTargetBlock (CodeProvider.FilterExpressionStart (handler));
					this.FilterCodeBlocks.Add (handler, targetBlock);
				}
			}
			return block;
		}

		public override IEnumerable<Subroutine> UsedSubroutines (HashSet<int> alreadySeen)
		{
			return this.FaultFinallySubroutines.Values.Concat (base.UsedSubroutines (alreadySeen));
		}

		public override LispList<Pair<EdgeTag, Subroutine>> EdgeSubroutinesOuterToInner (CFGBlock current, CFGBlock succ,
		                                                                                 out bool isExceptionHandlerEdge, LispList<Edge<CFGBlock, EdgeTag>> context)
		{
			if (current.Subroutine != this)
				return current.Subroutine.EdgeSubroutinesOuterToInner (current, succ, out isExceptionHandlerEdge, context);

			LispList<Handler> l1 = ProtectingHandlerList (current);
			LispList<Handler> l2 = ProtectingHandlerList (succ);
			isExceptionHandlerEdge = IsCatchFilterHeader (succ);

			LispList<Pair<EdgeTag, Subroutine>> result = GetOrdinaryEdgeSubroutines (current, succ, context);

			while (l1 != l2) {
				if (l1.Length () >= l2.Length ()) {
					Handler head = l1.Head;
					if (IsFaultOrFinally (head) && (!IsFault (head) || isExceptionHandlerEdge))
						result = result.Cons (new Pair<EdgeTag, Subroutine> (EdgeTag.Finally, this.FaultFinallySubroutines [head]));
					l1 = l1.Tail;
				} else
					l2 = l2.Tail;
			}

			return result;
		}

		private bool IsFaultOrFinally (Handler handler)
		{
			return CodeProvider.IsFaultHandler (handler) || CodeProvider.IsFinallyHandler (handler);
		}

		public override IEnumerable<Pair<Dummy, CFGBlock>> Successors (CFGBlock node)
		{
			foreach (var pair in SuccessorEdges [node])
				yield return new Pair<Dummy, CFGBlock> (Dummy.Value, pair.Value);

			foreach (Handler handler in ProtectingHandlerList (node).AsEnumerable ()) {
				if (!IsFaultOrFinally (handler))
					yield return new Pair<Dummy, CFGBlock> (Dummy.Value, this.CatchFilterHeaders [handler]);
			}
			if (node != ExceptionExit)
				yield return new Pair<Dummy, CFGBlock> (Dummy.Value, ExceptionExit);
		}

		public override IEnumerable<CFGBlock> ExceptionHandlers<Data, TType> (CFGBlock block, Subroutine innerSubroutine,
		                                                                      Data data, IHandlerFilter<Data> handlerPredicate)
		{
			IHandlerFilter<Data> handleFilter = handlerPredicate;
			LispList<Handler> protectingHandlers = ProtectingHandlerList (block);
			if (innerSubroutine != null && innerSubroutine.IsFaultFinally) {
				for (; protectingHandlers != null; protectingHandlers = protectingHandlers.Tail) {
					if (IsFaultOrFinally (protectingHandlers.Head) && this.FaultFinallySubroutines [protectingHandlers.Head] == innerSubroutine) {
						protectingHandlers = protectingHandlers.Tail;
						break;
					}
				}
			}

			for (; protectingHandlers != null; protectingHandlers = protectingHandlers.Tail) {
				Handler handler = protectingHandlers.Head;
				if (!IsFaultOrFinally (handler)) {
					if (handleFilter != null) {
						bool stopPropagation;
						if (CodeProvider.IsCatchHandler (handler)) {
							if (handleFilter.Catch (data, CodeProvider.CatchType (handler), out stopPropagation))
								yield return this.CatchFilterHeaders [handler];
						} else if (handleFilter.Filter (data, new APC (this.FilterCodeBlocks [handler], 0, null), out stopPropagation))
							yield return this.CatchFilterHeaders [handler];
						if (stopPropagation)
							yield break;
					} else
						yield return this.CatchFilterHeaders [handler];

					if (CodeProvider.IsCatchAllHandler (handler))
						yield break;
				}
			}
			yield return ExceptionExit;
		}

		protected override void PrintReferencedSubroutines (TextWriter tw, HashSet<Subroutine> subs, ILPrinter<APC> printer,
		                                                    Func<CFGBlock, IEnumerable<LispList<Edge<CFGBlock, EdgeTag>>>> contextLookup,
		                                                    LispList<Edge<CFGBlock, EdgeTag>> context,
		                                                    HashSet<Pair<Subroutine, LispList<Edge<CFGBlock, EdgeTag>>>> printed)
		{
			foreach (Subroutine sub in this.FaultFinallySubroutines.Values) {
				if (contextLookup == null)
					sub.Print (tw, printer, contextLookup, context, printed);
				else {
					foreach (var ctx in contextLookup (sub.Entry))
						sub.Print (tw, printer, contextLookup, ctx, printed);
				}
			}

			base.PrintReferencedSubroutines (tw, subs, printer, contextLookup, context, printed);
		}

		protected override void PrintHandlers (TextWriter tw, BlockWithLabels<Label> block)
		{
			tw.Write ("  Handlers: ");
			foreach (Handler handler in ProtectingHandlerList (block).AsEnumerable ()) {
				if (IsFaultOrFinally (handler))
					tw.Write ("SR{0} ", this.FaultFinallySubroutines [handler].Id);
				else
					tw.Write ("{0} ", this.CatchFilterHeaders [handler].Index);
			}
			if (block != ExceptionExit)
				tw.Write ("{0} ", ExceptionExit.Index);
			tw.WriteLine ();
		}
	}
}
