// 
// SubroutineBase.cs
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
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.ControlFlow.Blocks;
using Mono.CodeContracts.Static.ControlFlow.Subroutines.Builders;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines {
	abstract class SubroutineBase<Label> : Subroutine, IGraph<CFGBlock, Dummy>, IStackInfo, IEdgeSubroutineAdaptor {
		private const int UnusedBlockIndex = Int32.MaxValue - 1;
		protected readonly Label StartLabel;
		public readonly SubroutineFacade SubroutineFacade;

		private readonly Dictionary<Pair<CFGBlock, CFGBlock>, LispList<Pair<EdgeTag, Subroutine>>> edge_subroutines
			= new Dictionary<Pair<CFGBlock, CFGBlock>, LispList<Pair<EdgeTag, Subroutine>>> ();

		private readonly BlockWithLabels<Label> entry;
		private readonly BlockWithLabels<Label> entry_after_requires;
		private readonly BlockWithLabels<Label> exception_exit;
		private readonly BlockWithLabels<Label> exit;

		private readonly List<Edge<CFGBlock, EdgeTag>> successors = new List<Edge<CFGBlock, EdgeTag>> ();
		protected int BlockIdGenerator;
		protected Dictionary<Label, BlockWithLabels<Label>> LabelsThatStartBlocks = new Dictionary<Label, BlockWithLabels<Label>> ();

		private CFGBlock[] blocks;
		private DepthFirst.Visitor<CFGBlock, Dummy> edge_info;
		private EdgeMap<EdgeTag> predecessor_edges;
		private EdgeMap<EdgeTag> successor_edges;

		protected SubroutineBase (SubroutineFacade subroutineFacade)
		{
			this.SubroutineFacade = subroutineFacade;
			this.entry = new EntryBlock<Label> (this, ref this.BlockIdGenerator);
			this.exit = new EntryExitBlock<Label> (this, ref this.BlockIdGenerator);
			this.exception_exit = new CatchFilterEntryBlock<Label> (this, ref this.BlockIdGenerator);
		}

		protected SubroutineBase (SubroutineFacade SubroutineFacade,
		                          Label startLabel, SubroutineBuilder<Label> builder)
			: this (SubroutineFacade)
		{
			this.StartLabel = startLabel;
			Builder = builder;
			CodeProvider = builder.CodeProvider;
			this.entry_after_requires = GetTargetBlock (startLabel);

			AddSuccessor (this.entry, EdgeTag.Entry, this.entry_after_requires);
		}

		public override bool HasContextDependentStackDepth
		{
			get { return true; }
		}

		public override bool HasReturnValue
		{
			get { return false; }
		}

		public override int StackDelta
		{
			get { return 0; }
		}

		protected SubroutineBuilder<Label> Builder { get; set; }

		public override DepthFirst.Visitor<CFGBlock, Dummy> EdgeInfo
		{
			get { return this.edge_info; }
		}

		public ICodeProvider<Label> CodeProvider { get; private set; }

		public override int BlockCount
		{
			get { return this.blocks.Length; }
		}

		public override IEnumerable<CFGBlock> Blocks
		{
			get { return this.blocks; }
		}

		public override string Name
		{
			get { return "SR" + Id; }
		}

		public override EdgeMap<EdgeTag> SuccessorEdges
		{
			get { return this.successor_edges; }
		}

		public override EdgeMap<EdgeTag> PredecessorEdges
		{
			get
			{
				if (this.predecessor_edges == null)
					this.predecessor_edges = this.successor_edges.Reverse ();
				return this.predecessor_edges;
			}
		}

		#region Main Blocks
		public override CFGBlock Entry
		{
			get { return this.entry; }
		}

		public override CFGBlock EntryAfterRequires
		{
			get
			{
				if (this.entry_after_requires != null)
					return this.entry_after_requires;
				return Entry;
			}
		}

		public override CFGBlock Exit
		{
			get { return this.exit; }
		}

		public override CFGBlock ExceptionExit
		{
			get { return this.exception_exit; }
		}
		#endregion

		#region IEdgeSubroutineAdaptor Members
		public LispList<Pair<EdgeTag, Subroutine>> GetOrdinaryEdgeSubroutinesInternal (CFGBlock from, CFGBlock to, LispList<Edge<CFGBlock, EdgeTag>> context)
		{
			LispList<Pair<EdgeTag, Subroutine>> list;
			this.edge_subroutines.TryGetValue (new Pair<CFGBlock, CFGBlock> (from, to), out list);
			if (list != null && context != null)
				list = list.Where (FilterRecursiveContracts (to, context));
			return list;
		}
		#endregion

		#region IGraph<CFGBlock,Dummy> Members
		public IEnumerable<CFGBlock> Nodes
		{
			get { return this.blocks; }
		}

		public virtual IEnumerable<Pair<Dummy, CFGBlock>> Successors (CFGBlock node)
		{
			foreach (var pair in this.successor_edges [node])
				yield return new Pair<Dummy, CFGBlock> (Dummy.Value, pair.Value);

			if (node != this.exception_exit)
				yield return new Pair<Dummy, CFGBlock> (Dummy.Value, this.exception_exit);
		}
		#endregion

		#region IStackInfo Members
		bool IStackInfo.IsCallOnThis (APC apc)
		{
			return false;
		}
		#endregion

		public override IEnumerable<CFGBlock> SuccessorBlocks (CFGBlock block)
		{
			return SuccessorEdges [block].Select (it => it.Value);
		}

		public override bool HasSingleSuccessor (APC point, out APC ifFound)
		{
			if (point.Index < point.Block.Count) {
				ifFound = new APC (point.Block, point.Index + 1, point.SubroutineContext);
				return true;
			}

			if (IsSubroutineEnd (point.Block)) {
				if (point.SubroutineContext == null) {
					ifFound = APC.Dummy;
					return false;
				}

				ifFound = ComputeSubroutineContinuation (point);
				return true;
			}

			BlockWithLabels<Label> onlyOne = null;
			foreach (BlockWithLabels<Label> successor in point.Block.Subroutine.SuccessorBlocks (point.Block)) {
				if (onlyOne == null)
					onlyOne = successor;
				else {
					ifFound = APC.Dummy;
					return false;
				}
			}

			if (onlyOne != null) {
				ifFound = ComputeTargetFinallyContext (point, onlyOne);
				return true;
			}

			ifFound = APC.Dummy;
			return false;
		}

		public override bool HasSinglePredecessor (APC point, out APC ifFound)
		{
			if (point.Index > 0) {
				ifFound = new APC (point.Block, point.Index - 1, point.SubroutineContext);
				return true;
			}

			if (IsSubroutineStart (point.Block)) {
				if (point.SubroutineContext == null) {
					ifFound = APC.Dummy;
					return false;
				}

				bool hasSinglePredecessor;
				ifFound = ComputeSubroutinePreContinuation (point, out hasSinglePredecessor);
				return hasSinglePredecessor;
			}


			CFGBlock onlyOne = null;
			foreach (CFGBlock predecessor in point.Block.Subroutine.PredecessorBlocks (point.Block)) {
				if (onlyOne != null) {
					ifFound = APC.Dummy;
					return false;
				}
				onlyOne = predecessor;
			}
			if (onlyOne == null) {
				ifFound = APC.Dummy;
				return false;
			}

			LispList<Pair<EdgeTag, Subroutine>> list = EdgeSubroutinesOuterToInner (onlyOne, point.Block, point.SubroutineContext);
			if (list.IsEmpty ()) {
				ifFound = APC.ForEnd (onlyOne, point.SubroutineContext);
				return true;
			}

			var edge = new Edge<CFGBlock, EdgeTag> (onlyOne, point.Block, list.Head.Key);
			Subroutine sub = list.Head.Value;
			ifFound = APC.ForEnd (sub.Exit, point.SubroutineContext.Cons (edge));
			return true;
		}

		private APC ComputeSubroutinePreContinuation (APC point, out bool hasSinglePredecessor)
		{
			Edge<CFGBlock, EdgeTag> head = point.SubroutineContext.Head;
			bool isExceptionHandlerEdge;
			LispList<Edge<CFGBlock, EdgeTag>> tail = point.SubroutineContext.Tail;
			LispList<Pair<EdgeTag, Subroutine>> flist = EdgeSubroutinesOuterToInner (head.From, head.To, out isExceptionHandlerEdge, tail);
			while (flist.Head.Value != this)
				flist = flist.Tail;
			if (flist.Tail.IsEmpty ()) {
				if (isExceptionHandlerEdge && head.From.Count > 1) {
					hasSinglePredecessor = false;
					return APC.Dummy;
				}

				hasSinglePredecessor = true;
				return APC.ForEnd (head.From, tail);
			}
			Pair<EdgeTag, Subroutine> first = flist.Tail.Head;
			Subroutine sub = first.Value;
			hasSinglePredecessor = true;

			return APC.ForEnd (sub.Exit, point.SubroutineContext.Cons (new Edge<CFGBlock, EdgeTag> (head.From, head.To, first.Key)));
		}

		private APC ComputeSubroutineContinuation (APC point)
		{
			Edge<CFGBlock, EdgeTag> head = point.SubroutineContext.Head;
			LispList<Edge<CFGBlock, EdgeTag>> tail = point.SubroutineContext.Tail;
			LispList<Pair<EdgeTag, Subroutine>> outerToInner = EdgeSubroutinesOuterToInner (head.From, head.To, tail);
			if (outerToInner.Head.Value == this)
				return new APC (head.To, 0, tail);

			while (outerToInner.Tail.Head.Value != this)
				outerToInner = outerToInner.Tail;

			return new APC (outerToInner.Head.Value.Entry, 0, tail.Cons (new Edge<CFGBlock, EdgeTag> (head.From, head.To, outerToInner.Head.Key)));
		}

		public override IEnumerable<CFGBlock> PredecessorBlocks (CFGBlock block)
		{
			return PredecessorEdges [block].Select (it => it.Value);
		}

		public override bool IsJoinPoint (CFGBlock block)
		{
			if (IsCatchFilterHeader (block) || IsSubroutineStart (block) || IsSubroutineEnd (block))
				return true;

			return PredecessorEdges [block].Count > 1;
		}

		public override bool IsSubroutineEnd (CFGBlock block)
		{
			return block == this.exit || block == this.exception_exit;
		}

		public override bool IsSubroutineStart (CFGBlock block)
		{
			return block == this.entry;
		}

		public override bool IsSplitPoint (CFGBlock block)
		{
			if (IsSubroutineStart (block) || IsSubroutineEnd (block))
				return true;

			return SuccessorEdges [block].Count > 1;
		}

		public override bool IsCatchFilterHeader (CFGBlock block)
		{
			return block is CatchFilterEntryBlock<Label>;
		}

		public void AddSuccessor (CFGBlock from, EdgeTag tag, CFGBlock to)
		{
			AddNormalControlFlowEdge (this.successors, from, tag, to);
		}

		private void AddNormalControlFlowEdge (List<Edge<CFGBlock, EdgeTag>> succs, CFGBlock from, EdgeTag tag, CFGBlock to)
		{
			succs.Add (new Edge<CFGBlock, EdgeTag> (from, to, tag));
		}

		public virtual void AddReturnBlock (BlockWithLabels<Label> block)
		{
		}

		public BlockWithLabels<Label> GetTargetBlock (Label label)
		{
			return GetBlock (label);
		}

		public BlockWithLabels<Label> GetBlock (Label label)
		{
			IMetaDataProvider metadataDecoder = this.SubroutineFacade.MetaDataProvider;

			BlockWithLabels<Label> block;
			if (!this.LabelsThatStartBlocks.TryGetValue (label, out block)) {
				Pair<Method, bool> methodVirtualPair;
				Method constructor;

				if (Builder == null)
					throw new InvalidOperationException ("Builder must be not null");

				if (Builder.IsMethodCallSite (label, out methodVirtualPair)) {
					int parametersCount = metadataDecoder.Parameters (methodVirtualPair.Key).Count;
					block = new MethodCallBlock<Label> (methodVirtualPair.Key, this, ref this.BlockIdGenerator, parametersCount, methodVirtualPair.Value);
				} else if (Builder.IsNewObjSite (label, out constructor)) {
					int parametersCount = metadataDecoder.Parameters (constructor).Count;
					block = new NewObjCallBlock<Label> (constructor, parametersCount, this, ref this.BlockIdGenerator);
				} else
					block = NewBlock ();

				if (Builder.IsTargetLabel (label))
					this.LabelsThatStartBlocks.Add (label, block);
			}
			return block;
		}

		public virtual BlockWithLabels<Label> NewBlock ()
		{
			return new BlockWithLabels<Label> (this, ref this.BlockIdGenerator);
		}

		public AssumeBlock<Label> NewAssumeBlock (Label pc, EdgeTag tag)
		{
			return new AssumeBlock<Label> (this, pc, tag, ref this.BlockIdGenerator);
		}

		public override sealed void AddEdgeSubroutine (CFGBlock from, CFGBlock to, Subroutine subroutine, EdgeTag tag)
		{
			if (subroutine == null)
				return;

			var key = new Pair<CFGBlock, CFGBlock> (from, to);
			LispList<Pair<EdgeTag, Subroutine>> list;
			var item = new Pair<EdgeTag, Subroutine> (tag, subroutine);

			this.edge_subroutines.TryGetValue (key, out list);
			this.edge_subroutines [key] = list.Cons (item);
		}

		public override IEnumerable<APC> Successors (APC pc)
		{
			APC singleNext;
			if (HasSingleSuccessor (pc, out singleNext))
				yield return singleNext;
			else {
				foreach (CFGBlock block in pc.Block.Subroutine.SuccessorBlocks (pc.Block))
					yield return pc.Block.Subroutine.ComputeTargetFinallyContext (pc, block);
			}
		}

		public override IEnumerable<APC> Predecessors (APC pc)
		{
			if (pc.Index > 0)
				yield return new APC (pc.Block, pc.Index - 1, pc.SubroutineContext);

			else if (IsSubroutineStart (pc.Block)) {
				if (!pc.SubroutineContext.IsEmpty ()) {
					foreach (APC apc in ComputeSubroutinePreContinuation (pc))
						yield return apc;
				}
			} else {
				foreach (CFGBlock block in pc.Block.Subroutine.PredecessorBlocks (pc.Block)) {
					LispList<Pair<EdgeTag, Subroutine>> diffs = EdgeSubroutinesOuterToInner (block, pc.Block, pc.SubroutineContext);
					if (diffs.IsEmpty ())
						yield return APC.ForEnd (block, pc.SubroutineContext);
					else {
						Subroutine sub = diffs.Head.Value;
						var edge = new Edge<CFGBlock, EdgeTag> (block, pc.Block, diffs.Head.Key);
						yield return APC.ForEnd (sub.Exit, pc.SubroutineContext.Cons (edge));
					}
				}
			}
		}

		private IEnumerable<APC> ComputeSubroutinePreContinuation (APC point)
		{
			Edge<CFGBlock, EdgeTag> edge = point.SubroutineContext.Head;
			LispList<Edge<CFGBlock, EdgeTag>> tail = point.SubroutineContext.Tail;

			bool isHandlerEdge;
			LispList<Pair<EdgeTag, Subroutine>> diffs = EdgeSubroutinesOuterToInner (edge.From, edge.To, out isHandlerEdge, tail);
			while (diffs.Head.Value != this)
				diffs = diffs.Tail;

			if (diffs.Tail == null) {
				if (isHandlerEdge) {
					for (int i = 0; i < edge.From.Count; i++)
						yield return new APC (edge.From, i, tail);
				} else
					yield return APC.ForEnd (edge.From, tail);
			} else {
				Pair<EdgeTag, Subroutine> first = diffs.Tail.Head;
				Subroutine nextSubroutine = first.Value;
				yield return APC.ForEnd (nextSubroutine.Exit, point.SubroutineContext.Cons (new Edge<CFGBlock, EdgeTag> (edge.From, edge.To, first.Key)));
			}
		}

		public override APC ComputeTargetFinallyContext (APC pc, CFGBlock succ)
		{
			LispList<Pair<EdgeTag, Subroutine>> list = EdgeSubroutinesOuterToInner (pc.Block, succ, pc.SubroutineContext);
			if (list.IsEmpty ())
				return new APC (succ, 0, pc.SubroutineContext);

			Pair<EdgeTag, Subroutine> last = list.Last ();
			return new APC (last.Value.Entry, 0, pc.SubroutineContext.Cons (new Edge<CFGBlock, EdgeTag> (pc.Block, succ, last.Key)));
		}

		private LispList<Pair<EdgeTag, Subroutine>> EdgeSubroutinesOuterToInner (CFGBlock from, CFGBlock succ, LispList<Edge<CFGBlock, EdgeTag>> subroutineContext)
		{
			bool isExceptionHandlerEdge;
			return EdgeSubroutinesOuterToInner (from, succ, out isExceptionHandlerEdge, subroutineContext);
		}

		public override LispList<Pair<EdgeTag, Subroutine>> EdgeSubroutinesOuterToInner (CFGBlock from, CFGBlock succ, out bool isExceptionHandlerEdge, LispList<Edge<CFGBlock, EdgeTag>> context)
		{
			if (from.Subroutine != this)
				return from.Subroutine.EdgeSubroutinesOuterToInner (from, succ, out isExceptionHandlerEdge, context);

			isExceptionHandlerEdge = IsCatchFilterHeader (succ);
			return GetOrdinaryEdgeSubroutines (from, succ, context);
		}

		public override LispList<Pair<EdgeTag, Subroutine>> GetOrdinaryEdgeSubroutines (CFGBlock from, CFGBlock to, LispList<Edge<CFGBlock, EdgeTag>> context)
		{
			IMetaDataProvider metadataDecoder = this.SubroutineFacade.MetaDataProvider;
			var apc = new APC (to, 0, context);

			DecoratorHelper.Push (this);
			try {
				LispList<Pair<EdgeTag, Subroutine>> list = DecoratorHelper.Dispatch<IEdgeSubroutineAdaptor> (this).GetOrdinaryEdgeSubroutinesInternal (from, to, context);
				if (apc.InsideContract) {
					if (context != null && !list.IsEmpty ()) {
						Method calledMethod;
						bool isNewObj;
						bool isVirtual;
						if (@from.IsMethodCallBlock (out calledMethod, out isNewObj, out isVirtual) && isVirtual && ((IStackInfo) this).IsCallOnThis (new APC (@from, 0, null))) {
							TypeNode type = metadataDecoder.DeclaringType (calledMethod);
							do {
								if (context.Head.Tag.Is (EdgeTag.InheritedMask) || context.Head.Tag.Is (EdgeTag.ExtraMask) || context.Head.Tag.Is (EdgeTag.OldMask))
									context = context.Tail;
								else {
									Method calledMethod2;
									bool isNewObj2;
									bool isVirtual2;
									if (context.Head.Tag.Is (EdgeTag.AfterMask) && context.Head.From.IsMethodCallBlock (out calledMethod2, out isNewObj2, out isVirtual2)) {
										TypeNode sub = metadataDecoder.DeclaringType (calledMethod2);
										if (metadataDecoder.DerivesFrom (sub, type))
											type = sub;
										if (!DecoratorHelper.Dispatch<IStackInfo> (this).IsCallOnThis (new APC (context.Head.From, 0, null)))
											break;
									} else if (context.Head.Tag.Is (EdgeTag.BeforeMask) && context.Head.To.IsMethodCallBlock (out calledMethod2, out isNewObj2, out isVirtual2)) {
										TypeNode sub = metadataDecoder.DeclaringType (calledMethod2);
										if (metadataDecoder.DerivesFrom (sub, type))
											type = sub;
										if (!DecoratorHelper.Dispatch<IStackInfo> (this).IsCallOnThis (new APC (context.Head.To, 0, null)))
											break;
									} else if (context.Head.Tag == EdgeTag.Exit) {
										var methodInfo = context.Head.From.Subroutine as IMethodInfo;
										if (methodInfo != null) {
											TypeNode sub = metadataDecoder.DeclaringType (methodInfo.Method);
											if (metadataDecoder.DerivesFrom (sub, type))
												type = sub;
										}
										break;
									} else {
										if (context.Head.Tag != EdgeTag.Entry)
											return list;
										var methodInfo = context.Head.From.Subroutine as IMethodInfo;
										if (methodInfo != null) {
											TypeNode sub = metadataDecoder.DeclaringType (methodInfo.Method);
											if (metadataDecoder.DerivesFrom (sub, type))
												type = sub;
										}
										break;
									}
									context = context.Tail;
								}
							} while (!context.IsEmpty ());
							Method implementingMethod;
							if (!metadataDecoder.Equal (type, metadataDecoder.DeclaringType (calledMethod)) &&
							    metadataDecoder.TryGetImplementingMethod (type, calledMethod, out implementingMethod))
								list = SpecializedEnsures (list, this.SubroutineFacade.GetEnsures (calledMethod), this.SubroutineFacade.GetEnsures (implementingMethod));
						}
					}
				} else {
					Method calledMethod;
					bool isNewObj;
					bool isVirtual;
					if (@from.IsMethodCallBlock (out calledMethod, out isNewObj, out isVirtual)) {
						if (DecoratorHelper.Dispatch<IStackInfo> (this).IsCallOnThis (new APC (@from, 0, null))) {
							var methodInfo = @from.Subroutine as IMethodInfo;
							if (methodInfo != null) {
								TypeNode bestType = metadataDecoder.DeclaringType (methodInfo.Method);
								Method implementingMethod;
								if (isVirtual && metadataDecoder.TryGetImplementingMethod (bestType, calledMethod, out implementingMethod))
									list = SpecializedEnsures (list, this.SubroutineFacade.GetEnsures (calledMethod), this.SubroutineFacade.GetEnsures (implementingMethod));
								list = InsertInvariant (@from, list, calledMethod, ref bestType, context);
							}
						}
					}
				}
				return list;
			} finally {
				DecoratorHelper.Pop ();
			}
		}

		private LispList<Pair<EdgeTag, Subroutine>> InsertInvariant (CFGBlock from, LispList<Pair<EdgeTag, Subroutine>> list,
		                                                             Method calledMethod, ref TypeNode type,
		                                                             LispList<Edge<CFGBlock, EdgeTag>> context)
		{
			IMetaDataProvider metadataDecoder = this.SubroutineFacade.MetaDataProvider;

			Property property;
			if (metadataDecoder.IsPropertySetter (calledMethod, out property)
			    && (metadataDecoder.IsAutoPropertyMember (calledMethod) || WithinConstructor (from, context)))
				return list;

			if (metadataDecoder.IsConstructor (calledMethod))
				type = metadataDecoder.DeclaringType (calledMethod);

			Subroutine invariant = this.SubroutineFacade.GetInvariant (type);
			if (invariant != null) {
				var methodCallBlock = from as MethodCallBlock<Label>;
				if (methodCallBlock != null) {
					EdgeTag first = methodCallBlock.IsNewObj ? EdgeTag.AfterNewObj : EdgeTag.AfterCall;
					return list.Cons (new Pair<EdgeTag, Subroutine> (first, invariant));
				}
			}

			return list;
		}

		private bool WithinConstructor (CFGBlock current, LispList<Edge<CFGBlock, EdgeTag>> context)
		{
			return new APC (current, 0, context).InsideConstructor;
		}

		private LispList<Pair<EdgeTag, Subroutine>> SpecializedEnsures (LispList<Pair<EdgeTag, Subroutine>> subroutines,
		                                                                Subroutine toReplace, Subroutine specializedEnsures)
		{
			return subroutines.Select (pair => new Pair<EdgeTag, Subroutine> (pair.Key, pair.Value == toReplace ? specializedEnsures : pair.Value));
		}

		private static Predicate<Pair<EdgeTag, Subroutine>> FilterRecursiveContracts (CFGBlock from, LispList<Edge<CFGBlock, EdgeTag>> context)
		{
			return (candidate) => {
			       	Subroutine sub = candidate.Value;
			       	if (!sub.IsContract)
			       		return true;
			       	if (sub == @from.Subroutine)
			       		return false;
			       	if (context.Any (ctx => sub == ctx.From.Subroutine))
			       		return false;
			       	return true;
			       };
		}

		public abstract override void Initialize ();

		public virtual void Commit ()
		{
			PostProcessBlocks ();
		}

		protected void PostProcessBlocks ()
		{
			var blockStack = new Stack<CFGBlock> ();
			this.successor_edges = new EdgeMap<EdgeTag> (this.successors);
			this.edge_info = new DepthFirst.Visitor<CFGBlock, Dummy> (this, null, (block) => blockStack.Push (block), null);
			this.edge_info.VisitSubGraphNonRecursive (this.exception_exit);
			this.edge_info.VisitSubGraphNonRecursive (this.exit);
			this.edge_info.VisitSubGraphNonRecursive (this.entry);

			foreach (var successorEdge in this.successor_edges) {
				int idGen = UnusedBlockIndex;
				successorEdge.From.Renumber (ref idGen);
			}
			int idGen1 = 0;
			foreach (CFGBlock cfgBlock in blockStack)
				cfgBlock.Renumber (ref idGen1);

			SuccessorEdges.Filter ((e) => e.From.Index != UnusedBlockIndex);
			this.predecessor_edges = this.successor_edges.Reverse ();
			int finishTime = 0;
			var visitor = new DepthFirst.Visitor<CFGBlock, EdgeTag> (this.predecessor_edges, null, block => block.ReversePostOrderIndex = finishTime++, null);
			visitor.VisitSubGraphNonRecursive (this.exit);
			foreach (CFGBlock node in blockStack)
				visitor.VisitSubGraphNonRecursive (node);

			SuccessorEdges.Resort ();
			this.blocks = blockStack.ToArray ();
			this.LabelsThatStartBlocks = null;
			Builder = null;
		}

		public override IEnumerable<Subroutine> UsedSubroutines (HashSet<int> alreadySeen)
		{
			foreach (var list in this.edge_subroutines.Values) {
				foreach (var pair in list.AsEnumerable ()) {
					Subroutine sub = pair.Value;
					if (!alreadySeen.Contains (sub.Id)) {
						alreadySeen.Add (sub.Id);
						yield return sub;
					}
				}
			}
		}

		public override IEnumerable<CFGBlock> ExceptionHandlers<Data, TType> (CFGBlock block, Subroutine innerSubroutine,
		                                                                      Data data, IHandlerFilter<Data> handlerPredicate)
		{
			yield return this.exception_exit;
		}


		public override void Print (TextWriter tw, ILPrinter<APC> printer, Func<CFGBlock,
		                                                                   	IEnumerable<LispList<Edge<CFGBlock, EdgeTag>>>> contextLookup,
		                            LispList<Edge<CFGBlock, EdgeTag>> context,
		                            HashSet<Pair<Subroutine, LispList<Edge<CFGBlock, EdgeTag>>>> printed)
		{
			var element = new Pair<Subroutine, LispList<Edge<CFGBlock, EdgeTag>>> (this, context);
			if (printed.Contains (element))
				return;
			printed.Add (element);
			var subs = new HashSet<Subroutine> ();
			var methodInfo = this as IMethodInfo;
			string method = (methodInfo != null) ? String.Format ("({0})", this.SubroutineFacade.MetaDataProvider.FullName (methodInfo.Method)) : null;

			tw.WriteLine ("Subroutine SR{0} {1} {2}", Id, Kind, method);
			tw.WriteLine ("-------------");
			foreach (BlockWithLabels<Label> block in this.blocks) {
				tw.Write ("Block {0} ({1})", block.Index, block.ReversePostOrderIndex);
				if (this.edge_info.DepthFirstInfo (block).TargetOfBackEdge)
					tw.WriteLine (" (target of backedge)");
				else if (IsJoinPoint (block))
					tw.WriteLine (" (join point)");
				else
					tw.WriteLine ();

				tw.Write ("  Predecessors: ");
				foreach (var edge in block.Subroutine.PredecessorEdges [block])
					tw.Write ("({0}, {1}) ", edge.Key, edge.Value.Index);
				tw.WriteLine ();
				PrintHandlers (tw, block);

				tw.WriteLine ("  Code:");
				foreach (APC apc in block.APCs ())
					printer (apc, "    ", tw);

				tw.Write ("  Successors: ");
				foreach (var edge in block.Subroutine.SuccessorEdges [block]) {
					tw.Write ("({0}, {1}", edge.Key, edge.Value.Index);
					if (this.edge_info.IsBackEdge (block, Dummy.Value, edge.Value))
						tw.Write (" BE");

					for (LispList<Pair<EdgeTag, Subroutine>> list = GetOrdinaryEdgeSubroutines (block, edge.Value, context); list != null; list = list.Tail) {
						subs.Add (list.Head.Value);
						tw.Write (" SR{0}({1})", list.Head.Value.Id, list.Head.Key);
					}
					tw.Write (") ");
				}
				tw.WriteLine ();
			}
			PrintReferencedSubroutines (tw, subs, printer, contextLookup, context, printed);
		}

		protected virtual void PrintReferencedSubroutines (TextWriter tw, HashSet<Subroutine> subs, ILPrinter<APC> printer,
		                                                   Func<CFGBlock, IEnumerable<LispList<Edge<CFGBlock, EdgeTag>>>> contextLookup,
		                                                   LispList<Edge<CFGBlock, EdgeTag>> context,
		                                                   HashSet<Pair<Subroutine, LispList<Edge<CFGBlock, EdgeTag>>>> printed)
		{
			foreach (Subroutine subroutine in subs) {
				if (contextLookup == null)
					subroutine.Print (tw, printer, contextLookup, context, printed);
				else {
					foreach (var ctx in contextLookup (subroutine.Entry))
						subroutine.Print (tw, printer, contextLookup, ctx, printed);
				}
			}
		}

		protected virtual void PrintHandlers (TextWriter tw, BlockWithLabels<Label> block)
		{
			tw.Write ("  Handlers: ");
			if (block != ExceptionExit)
				tw.Write ("{0} ", ExceptionExit.Index);
			tw.WriteLine ();
		}

		public int GetILOffset (Label label)
		{
			return CodeProvider.GetILOffset (label);
		}
	}
}
